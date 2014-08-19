using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XcodePostProcessLib;

using Debug = UnityEngine.Debug;


public static class XcodePostProcess
{
	[PostProcessBuild(int.MaxValue)]
	public static void PostProcess(BuildTarget target, string pathToBuiltProject)
	{	
		// Only iOS is supported right now - it should be fairly trivial to update
		// it to support Mac as well
		if (target != BuildTarget.iPhone)
			return;
		
		string xcodePath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");
		using (XcodeProject project = new XcodeProject(xcodePath)) 
		{
			string pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Plugins");
			string[] pluginFolders = Directory.GetDirectories(pluginsPath);
			
			foreach (string folder in pluginFolders)
			{
				string path = Path.Combine(pluginsPath, folder);
				string settingsPlistPath = Path.Combine(path, "XcodeSettings.plist");
				if (File.Exists(settingsPlistPath))
				{
					AddPlugin(path, project);
				}
			}
		}
	}
	
	private static void AddPlugin(string path, XcodeProject project)
	{
		PluginPostProcess plugin = new PluginPostProcess(project, path);
		plugin.PostProcess();
	}
	
	
	private class PluginPostProcess
	{
		private static string[] bundleExtensions = new string[] { ".bundle", ".app", ".lproj", ".framework" };


		private XcodeProject project;
		private string pluginPath;
		
		private Dictionary<string, PlistObject> settings;
		private bool forceLoadLibraries;
		private List<Regex> ignoreRegexes;
		
		public PluginPostProcess(XcodeProject xcodeProject, string path)
		{
			project = xcodeProject;
			pluginPath = path;
		}
		
		public void PostProcess()
		{
			string settingsPath = Path.Combine(pluginPath, "XcodeSettings.plist");
			settings = XmlPlistParser.Parse(settingsPath) as Dictionary<string, PlistObject>;
			
			if (settings.ContainsKey("forceLoadLibraries"))
				forceLoadLibraries = settings["forceLoadLibraries"].Bool;

			if (settings.ContainsKey("ignoredFilesRegexes"))
				ignoreRegexes = settings["ignoredFilesRegexes"].Array.Select(p => new Regex(p.String)).ToList();
			else
				ignoreRegexes = new List<Regex>();

			AddSystemLibs();
			AddBuildSettings();
			
			string pluginName = Path.GetFileName(pluginPath);
			XcodeGroup pluginGroup = project.CreateGroup(pluginName, project.RootGroup);
			
			AddPluginFiles(pluginGroup, pluginPath);
		}
		
		private void AddSystemLibs()
		{
			List<PlistObject> systemLibs = settings["systemLibraries"].Raw as List<PlistObject>;
			if (systemLibs != null)
			{
				foreach (PlistObject plistItem in systemLibs)
				{
					project.AddSystemLibrary(plistItem.String);
				}
			}
			
			
			List<PlistObject> weakSystemLibs = settings["weakSystemLibraries"].Raw as List<PlistObject>;
			if (weakSystemLibs != null)
			{
				foreach (PlistObject plistItem in weakSystemLibs)
				{
					project.AddSystemLibrary(plistItem.String, true);
				}
			}
		}
		
		private void AddBuildSettings()
		{
			Dictionary<string, PlistObject> buildSettings = settings["buildSettings"] as Dictionary<string, PlistObject>;
			if (buildSettings == null)
				return;
			
			foreach (var kvp in buildSettings)
			{
				if (kvp.Value.Raw is List<PlistObject>)
				{
					List<PlistObject> values = kvp.Value as List<PlistObject>;
					foreach (var value in values)
					{
						project.AppendBuildConfigurationSettingValue(kvp.Key, value.String);
					}
				}
				else
				{
					project.SetBuildConfigurationSetting(kvp.Key, kvp.Value);
				}
			}
		}
		
		private void AddPluginFiles(XcodeGroup parentGroup, string path)
		{
			var files = Directory.GetFiles(path).Where(ShouldIncludeFile);
			foreach (string file in files)
				AddFile(file, parentGroup);
			
			foreach (string dir in Directory.GetDirectories(path))
			{
				if (ShouldSearchDirectory(dir))
				{
					XcodeGroup subgroup = project.CreateGroup(Path.GetFileName(dir), parentGroup);
					AddPluginFiles(subgroup, dir);
				}
				
				if (ShouldIncludeDirectory(dir))
					AddFile(dir, parentGroup);
			}
		}
		
		private void AddFile(string path, XcodeGroup parentGroup)
		{
			project.AddFile(path, parentGroup);
			
			if (forceLoadLibraries)
			{
				FileType fileType = FileTypes.GetFileType(path);
				if (fileType == FileType.Framework)
				{
					// the ".a" file of a framework is in the root directory of the .framework folder,
					// named the same as the framework name
					//	ex.  Foundation.framework -> Foundation.framework/Foundation
					string frameworkLibPath = Path.Combine(path, Path.GetFileNameWithoutExtension(path));
					
					project.AppendBuildConfigurationSettingValue("OTHER_LDFLAGS", "-force_load");
					project.AppendBuildConfigurationSettingValue("OTHER_LDFLAGS", "\"" + frameworkLibPath + "\"");
				}
				else if (fileType == FileType.StaticLibrary)
				{
					project.AppendBuildConfigurationSettingValue("OTHER_LDFLAGS", "-force_load");
					project.AppendBuildConfigurationSettingValue("OTHER_LDFLAGS", "\"" + path + "\"");
				}
			}
		}

		private bool IgnoredFilesContainsPath(string filePath)
		{
			string pluginRelativePath = filePath.Substring(pluginPath.Length + 1);
			return (ignoreRegexes.FirstOrDefault(r => r.IsMatch(pluginRelativePath)) != null);
		}
		
		private bool ShouldIncludeFile(string filePath)
		{
			if (IgnoredFilesContainsPath(filePath))
				return false;

			string name = Path.GetFileName(filePath);
			if (name.StartsWith("."))
				return false;
			
			if (name == "XcodeSettings.plist")
				return false;
			
			// keep .js files, might be used in conjunction with UIWebView
			string[] ignoredFileExtensions = new string[] { ".meta", ".cs", ".boo" };
			if (ignoredFileExtensions.Contains(Path.GetExtension(filePath)))
				return false;
			
			return true;
		}
		
		private bool ShouldIncludeDirectory(string dirPath)
		{
			if (IgnoredFilesContainsPath(dirPath))
				return false;

			string name = Path.GetFileName(dirPath);
			if (name.StartsWith("."))
				return false;
			return bundleExtensions.Contains(Path.GetExtension(dirPath));
		}
		
		private bool ShouldSearchDirectory(string dirPath)
		{
			if (IgnoredFilesContainsPath(dirPath))
				return false;

			string name = Path.GetFileName(dirPath);
			if (name.StartsWith("."))
				return false;
			
			if (bundleExtensions.Contains(Path.GetExtension(name)))
				return false;
			
			return true;
		}
	
	}
	
}
