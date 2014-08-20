using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace XcodePostProcessLib
{
	public class XcodeProject : IDisposable
	{
		// Constants
		private const string PBXGroup = "PBXGroup";
		private const string RootGroupName = "CustomTemplate";
		
		// File info
		private PlistObject projectPlist;
		private string path;
		private ProjectSourceEditor projectSourceEditor;

		// Cached references
		private XcodeGroup rootGroup;
		private List<string> appBuildConfigurations;
		
		// other
		private bool disposed;
		
		// Constructors/Desctructors
		public XcodeProject(string filePath)
		{
			string xml = ProcessExec.Exec("/usr/libexec/PlistBuddy", "-x -c 'Print' '" + filePath + "'");
			projectPlist = XmlPlistParser.ParseString(xml);
			if (projectPlist == null)
				throw new Exception("Project plist wasn't a dictionary");
			
			path = filePath;
			projectSourceEditor = new ProjectSourceEditor(File.ReadAllText(filePath));
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				if (projectSourceEditor.IsValid) {
					File.WriteAllText(path, projectSourceEditor.Contents);
				} else {
					using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
						XmlPlistWriter.Write(projectPlist, fs);
				}
				disposed = true;
			}
		}
		
		~XcodeProject() 
		{
			Dispose();
		}
		
		// Properties
		public XcodeGroup RootGroup
		{
			get
			{
				if (rootGroup == null)
				{
					var pair = projectPlist["objects"]
						.Dict
						.Where(kvp => kvp.Value["isa"].String == PBXGroup && kvp.Value["name"].String == RootGroupName)
						.First();
					
					rootGroup = new XcodeGroup(pair.Key);
				}
				return rootGroup;
			}
		}
		
		private List<string> AppBuildConfigurations
		{
			get
			{
				if (appBuildConfigurations == null)
				{				
					// PBXNativeTarget->buildConfigurationList
					var configListIDs = GetObjectIDsForInstancesOfClass("PBXNativeTarget")
						.Select(id => projectPlist["objects"][id]["buildConfigurationList"].String);
					
					// XCConfigurationList->buildConfigurations
					appBuildConfigurations = GetObjectIDsForInstancesOfClass("XCConfigurationList")
						.Where(id => configListIDs.Contains(id))
						.SelectMany(id => projectPlist["objects"][id]["buildConfigurations"].Array.Select(plist => plist.String))
						.ToList();
				}
				return appBuildConfigurations;
			}
		}
			
		
		public XcodeGroup CreateGroup(string name, XcodeGroup parentGroup)
		{
			// if the group already exists, just return it instead of creating a new one
			XcodeGroup existing = FindGroup(name, parentGroup);
			if (existing != null)
				return existing;

			string groupTemplate = "\n" +
	@"		{0} /* {1} */ = {{
				isa = PBXGroup;
				children = (
				);
				name = {1};
				sourceTree = ""<group>"";
			}};";
			
			string objectId = GetUniqueObjectID();
			string groupDef = string.Format(groupTemplate, objectId, name);
			
			// write to plist
			projectPlist["objects"][objectId] = new Dictionary<string, PlistObject>()
			{
				{ "isa", PBXGroup },
				{ "children", new List<PlistObject>() },
				{ "name", name },
				{ "sourceTree", "<group>" }
			};
			
			// write to string
			projectSourceEditor.Modify((projectSource) => {
				int sectionEnd = projectSource.IndexOf("/* End PBXGroup section */");
				if (sectionEnd == -1)
					return null;
			
				return projectSource.Insert(sectionEnd - 1, groupDef);
			});

			XcodeGroup newGroup = new XcodeGroup(objectId);
			AddGroupChild(parentGroup, newGroup, name);
				
			return newGroup;
		}
		
		public void AddFile(string path, XcodeGroup parentGroup)
		{
			if (IsFileAdded(path))
				return;

			FileType fileType = FileTypes.GetFileType(path);
			string xcodeFileType = FileTypes.GetXcodeIdentifier(fileType);
			FileBuildPhase buildPhase = FileTypes.GetBuildPhase(fileType);
			
			XcodeFileRef fileRef = AddFileReference(path, xcodeFileType, parentGroup);
			if (buildPhase != FileBuildPhase.Unknown) 
			{
				string fileName = Path.GetFileName(path);
				AddBuildFile(fileRef, fileName, buildPhase);
			}
			
			if (buildPhase == FileBuildPhase.Frameworks)
			{
				string dir = Path.GetDirectoryName(path);
				string quoted = "\"" + dir.Replace("\"", "\\\"") + "\"";
				AppendBuildConfigurationSettingValue("FRAMEWORK_SEARCH_PATHS", quoted);
				AppendBuildConfigurationSettingValue("LIBRARY_SEARCH_PATHS", quoted);
			}
		}
		
		public void AddSystemLibrary(string libName, bool weakLink = false)
		{
			if (IsSystemLibraryLinked(libName))
				return;
			
			FileType fileType = FileTypes.GetFileType(libName);
			
			string path;
			if (fileType == FileType.Framework)
				path = "System/Library/Frameworks/" + libName;
			else if (fileType == FileType.DynamicLibrary)
				path = "usr/lib/" + libName;
			else
				throw new Exception("Library needs to be a framework or dylib (was: " + libName + ")");
			
			XcodeFileRef fileRef = AddFileReference(path, FileTypes.GetXcodeIdentifier(fileType), RootGroup, "SDKROOT");
			AddBuildFile(fileRef, libName, FileBuildPhase.Frameworks, weakLink);
	 	}
		
		private void AddGroupChild(XcodeGroup parent, XcodeHandle child, string childDescription)
		{
			// write to plist
			projectPlist["objects"][parent.Id]["children"].Array.Add(child.Id);
			
			// write to string
			projectSourceEditor.Modify((projectSource) => {
				Match match = Regex.Match(
					projectSource, 
					parent.Id + " /\\* .*\n\t*isa = PBXGroup;\n\t*children = \\(\n(\t*[A-Z0-9]{24} /\\* .*[^\n] \\*/,\n)*", 
					RegexOptions.Multiline
				);
				
				if (!match.Success)
					return null;
				
				int insertAt = match.Index + match.Length;
				return projectSource.Insert(insertAt, "\t\t\t\t" + child.Id + " /* " + childDescription + " */,\n");
			});
		}
		
		private XcodeFileRef AddFileReference(string fullPath, string fileType, XcodeGroup parentGroup, string sourceTree = "<absolute>")
		{
			// 0 - id, 1 - file name, 2 - file type, 3 - full path, 4 - source tree
			string format = "\n\t\t{0} /* {1} */ = {{isa = PBXFileReference; fileEncoding = 4; lastKnownFileType = \"{2}\"; name = \"{1}\"; path = \"{3}\"; sourceTree = \"{4}\"; }};";
			
			string objectId = GetUniqueObjectID();
			string name = Path.GetFileName(fullPath);
			string fileRefDef = string.Format(format, objectId, name, fileType, fullPath, sourceTree);
			
			// write to plist
			projectPlist["objects"][objectId] = new Dictionary<string, PlistObject>()
			{
				{ "isa", "PBXFileReference" },
				{ "fileEncoding", 4 },
				{ "lastKnownFileType", fileType },
				{ "name", name },
				{ "path", fullPath },
				{ "sourceTree", sourceTree },
			};
			
			// write to string
			projectSourceEditor.Modify((projectSource) => {
				int sectionEnd = projectSource.IndexOf("/* End PBXFileReference section */");
				if (sectionEnd == -1)
					return null;
				
				return projectSource.Insert(sectionEnd - 1, fileRefDef);
			});

			XcodeFileRef fileRef = new XcodeFileRef(objectId);
			AddGroupChild(parentGroup, fileRef, name);
			
			return fileRef;
		}
		
		private void AddBuildFile(XcodeFileRef fileRef, string fileName, FileBuildPhase buildPhase, bool weakAttribute = false)
		{
			// 0 - id, 1 - fileRefId, 2 - file name, 3 - build phase name, 4 settingsStr
			string format = "\n\t\t{0} /* {2} in {3} */ = {{isa = PBXBuildFile; fileRef = {1} /* {2} */;{4} }};";
			
			string objectId = GetUniqueObjectID();
			string settingsStr = weakAttribute ? " settings = {ATTRIBUTES = (Weak, ); };" : string.Empty;
			string buildFileDef = string.Format(format, objectId, fileRef.Id, fileName, FileTypes.GetHumanizedBuildPhase(buildPhase), settingsStr);
			
			// write to plist
			projectPlist["objects"][objectId] = new Dictionary<string, PlistObject>()
			{
				{ "isa", "PBXBuildFile" },
				{ "fileRef", fileRef.Id },
			};
			if (weakAttribute)
			{
				projectPlist["objects"][objectId].Dict.Add("settings", new Dictionary<string, PlistObject>()
				{
					{ "ATTRIBUTES", new List<PlistObject>() { "Weak" } }
				});
			}
			
			// write to string
			projectSourceEditor.Modify((projectSource) => {
				int sectionEnd = projectSource.IndexOf("/* End PBXBuildFile section */");
				if (sectionEnd == -1)
					return null;
				
				return projectSource.Insert(sectionEnd - 1, buildFileDef);
			});

			XcodeBuildFile buildFile = new XcodeBuildFile(objectId);
			AddBuildFileToPhase(buildFile, fileName, buildPhase);
		}
		
		private void AddBuildFileToPhase(XcodeBuildFile buildFile, string fileName, FileBuildPhase phase)
		{
			string buildPhaseClass;
			switch (phase)
			{
			case FileBuildPhase.Sources: buildPhaseClass = "PBXSourcesBuildPhase"; break;
			case FileBuildPhase.Frameworks: buildPhaseClass = "PBXFrameworksBuildPhase"; break;
			case FileBuildPhase.Resources: buildPhaseClass = "PBXResourcesBuildPhase"; break;	
			default: return; // exit, as we can't do anything without a buildPhaseClass
			}
			
			string phaseId = GetObjectIDForInstanceOfClass(buildPhaseClass);
			
			// write to plist
			projectPlist["objects"][phaseId]["files"].Array.Add(buildFile.Id);
			
			// write to string
			projectSourceEditor.Modify((projectSource) => {
				Match match = Regex.Match(
					projectSource, 
					phaseId + " /\\* .*\n\t*isa = " + buildPhaseClass + ";\n\t*buildActionMask = [0-9]+;\n\t*files = \\(\n(\t*[A-Z0-9]{24} /\\* .*[^\n] \\*/,\n)*", 
					RegexOptions.Multiline
				);
				
				if (!match.Success)
					return null;
				
				int insertAt = match.Index + match.Length;
				return projectSource.Insert(insertAt, "\t\t\t\t" + buildFile.Id + " /* " + fileName + " */,\n");
			});
		}

		private XcodeGroup FindGroup(string name, XcodeGroup parentGroup)
		{
			IEnumerable<string> childrenIds = projectPlist["objects"][parentGroup.Id]["children"].Array.Select(plist => plist.String);

			string groupId = childrenIds.Where(id => {
				Dictionary<string, PlistObject> obj = projectPlist["objects"][id].Dict;
				if (obj["isa"] != "PBXGroup")
					return false;

				return obj.ContainsKey("name") && obj["name"] == name;
			}).FirstOrDefault();

			if (groupId != null) {
				return new XcodeGroup(groupId);
			} else {
				return null;
			}
		}

		private bool IsFileAdded(string filePath)
		{
			List<string> fileReferences = GetObjectIDsForInstancesOfClass("PBXFileReference");
			return fileReferences
				.Select(objId => projectPlist["objects"][objId].Dict)
				.Where(dict => (string)dict["path"] == filePath)
				.Count() > 0;
		}

		private bool IsSystemLibraryLinked(string libName)
		{
			string objectId = GetObjectIDForInstanceOfClass("PBXFrameworksBuildPhase");
			
			if (objectId == null)
				throw new Exception("PBXFrameworksBuildPhase not found");
			
			foreach (PlistObject item in projectPlist["objects"][objectId]["files"].Array)
			{
				string pbxBuildFileId = item.String;
				string fileRefId = projectPlist["objects"][pbxBuildFileId]["fileRef"].String;
				
				Dictionary<string, PlistObject> fileInfo = projectPlist["objects"][fileRefId].Dict;
				if (fileInfo.ContainsKey("name"))
				{
					if (fileInfo["name"] == libName)
						return true;
				}
			}
			
			return false;
		}
		
		// Use for single value build settings (i.e INFOPLIST_FILE = Info.plist)
		// Creates entry if it doesn't exist
		public void SetBuildConfigurationSetting(string key, string value, string limitedToObjectID = null)
		{		
			List<string> configurations = limitedToObjectID != null ?
				new List<string>() { limitedToObjectID } :
				AppBuildConfigurations;

			foreach (string objectId in configurations)
			{
				Dictionary<string, PlistObject> buildSettings = projectPlist["objects"][objectId]["buildSettings"];

				projectSourceEditor.Modify((projectSource) => {
					if (buildSettings.ContainsKey(key))
					{
						if (buildSettings[key].Raw is Dictionary<string, PlistObject>)
							throw new Exception("Invalid buildSettings key/value"); // shouldn't ever be a dictionary
						
						// we use the range of group 'valueMatchGroup' in 'match' to replace the existing value with our new one
						Match match;
						int valueMatchGroup;
						if (buildSettings[key].Raw is List<PlistObject>)
						{
							match = Regex.Match(
								projectSource, 
								objectId + " /\\* .*\n\t*isa = XCBuildConfiguration;\n\t*buildSettings = \\{\n(\t{4,}[^\n]*\n)*\t{4}\"{0,1}" + key + "\"{0,1} = (\\(\n(\t{5,}.*,\n)*\\\t{4}\\);)", 
								RegexOptions.Multiline
							);
							valueMatchGroup = 2;
						}
						else
						{
							// existing build setting, we use the regex to find the 'value' part of the key value pair, and then
							// use that range to replace it with our new value
							match = Regex.Match(
								projectSource, 
								objectId + " /\\* .*\n\t*isa = XCBuildConfiguration;\n\t*buildSettings = \\{\n(\t{4,}[^\n]*\n)*\t{4}\"{0,1}" + key + "\"{0,1} = ([^\n]*)", 
								RegexOptions.Multiline
							);
							valueMatchGroup = 2;
						}
						
						var valueGroup = match.Groups[valueMatchGroup];
						if (!match.Success || !valueGroup.Success)
							return null;
						
						projectSource = projectSource
							.Remove(valueGroup.Index, valueGroup.Length)
							.Insert(valueGroup.Index, "\"" + value.Replace("\"", "\\\"") + "\";");
					}
					else
					{
						// new build setting, find the end of the buildSettings list and insert our key/value pair there
						Match match = Regex.Match(
							projectSource, 
							objectId + " /\\* .*\n\t*isa = XCBuildConfiguration;\n\t*buildSettings = \\{\n(\t{4,}[^\n]*\n)*", 
							RegexOptions.Multiline
						);
						
						if (!match.Success)
							return null;
				
						int insertAt = match.Index + match.Length;
						projectSource = projectSource.Insert(insertAt, "\t\t\t\t\"" + key + "\" = \"" + value.Replace("\"", "\\\"") + "\";\n");
					}

					return projectSource;
				});
				
				buildSettings[key] = value;
			}
		}
		
		// Use for array build settings (i.e. OTHER_LDFLAGS = ("-ObjC", "-all_load"))
		// Creates entry if it doesn't exist
		public void AppendBuildConfigurationSettingValue(string key, string value)
		{
			List<string> configurations = AppBuildConfigurations;
			foreach (string objectId in configurations)
			{
				Dictionary<string, PlistObject> buildSettings = projectPlist["objects"][objectId]["buildSettings"];
				
				if (!buildSettings.ContainsKey(key))
				{
					// if it doesn't exist, create as a single key/value pair.  if we append another item, we'll convert it
					// to an array later (this mimics Xcode's behaviour)
					SetBuildConfigurationSetting(key, value, objectId);
					continue;
				} 
				else if ((buildSettings[key].Raw is List<PlistObject>) == false)
				{
					// existing, but not an array, we'll need to convert it
					
					if (buildSettings[key].Raw is Dictionary<string, PlistObject>)
						throw new Exception("Invalid buildSettings key/value"); // shouldn't ever be a dictionary
					
					// convert
					// 	SETTING_NAME = "value";
					// to 
					// 	SETTING_NAME = (
					//		"value",
					//	);

					var prevValue = buildSettings[key].Raw.ToString();

					projectSourceEditor.Modify((projectSource) => {
						Match m = Regex.Match(
							projectSource, 
							objectId + " /\\* .*\n\t*isa = XCBuildConfiguration;\n\t*buildSettings = \\{\n(\t{4,}[^\n]*\n)*\t{4}\"{0,1}" + key + "\"{0,1} = ([^\n]*)", 
							RegexOptions.Multiline
						);
						
						var valueGroup = m.Groups[2];
						if (!m.Success || !valueGroup.Success)
							return null;

						return projectSource
							.Remove(valueGroup.Index, valueGroup.Length)
							.Insert(valueGroup.Index, "(\n\t\t\t\t\t\"" + prevValue.Replace("\"", "\\\"") + "\",\n\t\t\t\t);");
					});

					buildSettings[key] = new List<PlistObject>();
					buildSettings[key].Array.Add(prevValue);
				}
					
				// find the build setting array in the file and append our item
				projectSourceEditor.Modify((projectSource) => {
					Match match = Regex.Match(
						projectSource,
						objectId + " /\\* .*\n\t*isa = XCBuildConfiguration;\n\t*buildSettings = \\{\n(\t{4,}[^\n]*\n)*\t{4}\"{0,1}" + key + "\"{0,1} = \\(\n(\t{5,}.*,\n)*", 
						RegexOptions.Multiline
					);
					
					if (!match.Success)
						return null;
					
					return projectSource
						.Insert(match.Index + match.Length, "\t\t\t\t\t\"" + value.Replace("\"", "\\\"") + "\",\n");
				});

				buildSettings[key].Array.Add(value);
			}
		}
		
		private string GetUniqueObjectID()
		{
			var objects = projectPlist["objects"].Dict;
			string id = GenerateObjectID();
			
			while (objects.ContainsKey(id))
				id = GenerateObjectID();
			
			return id;
		}
		
		private string GenerateObjectID()
		{
			// Object IDs are 12 bytes long, saved as a 24 character upper case string
			const int Len = 12;
			byte[] bytes = new byte[Len];
			
			Random r = new Random();
			for (int i = 0; i < Len; i++) 
			{
				bytes[i] = (byte)r.Next(0, 256);
			}
			
			return string.Format(
				"{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}" +
				"{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}{11:X2}",
				bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], 
				bytes[6], bytes[7], bytes[8], bytes[9], bytes[10], bytes[11]
			);
		}
		
		// Returns the object ID for any instance of klass (or null on failure).  Realistically should only be
		// used for objects that have only 1 instance.
		private List<string> GetObjectIDsForInstancesOfClass(string klass)
		{
			return ((Dictionary<string, PlistObject>)projectPlist["objects"])
				.AsEnumerable()
				.Where(p => p.Value["isa"] == klass)
				.Select(p => p.Key)
				.ToList();
		}
		
		// Returns the object ID for any instance of klass (or null on failure).  Realistically should only be
		// used for objects that have only 1 instance.
		private string GetObjectIDForInstanceOfClass(string klass)
		{
			return ((Dictionary<string, PlistObject>)projectPlist["objects"])
				.AsEnumerable()
				.Where(p => p.Value["isa"] == klass)
				.Select(p => p.Key)
				.FirstOrDefault();
		}
	}
}