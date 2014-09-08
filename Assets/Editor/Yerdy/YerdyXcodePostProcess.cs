using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using XCodeEditorYerdy;


public static class YerdyXcodePostProcess
{
	private const string iOSFilesPath = "Assets/Plugins/Yerdy/iOS";


	private const bool WEAK = true, 
					   STRONG = false;
	private static readonly Dictionary<string, bool> Frameworks = new Dictionary<string, bool>()
	{
		{ "StoreKit.framework", 			STRONG },
		{ "SystemConfiguration.framework", 	STRONG },
		{ "AdSupport.framework", 			WEAK },
	};


	[PostProcessBuild]
	public static void YerdyPostProcessBuild(BuildTarget target, string path) 
	{
		if (target != BuildTarget.iPhone)
			return;

		XCProject project = new XCProject(path);

		// header search path
		project.AddHeaderSearchPaths(iOSFilesPath + "/**");

		// add files
		var group = project.GetGroup("Yerdy");
		var allFiles = Directory.GetFiles(iOSFilesPath, "*.*", SearchOption.AllDirectories);

		var sourceFiles = allFiles.Where(f => f.EndsWith(".h") || f.EndsWith(".mm") || f.EndsWith(".m"));
		foreach (var f in sourceFiles)
			project.AddFile(f, group);

		var libs = allFiles.Where(f => f.EndsWith(".a"));
		foreach (var f in libs) {
			project.AddFile(f, group, "SOURCE_ROOT", false); // don't add build files, since we are 'force_load'-ing them

			var fullPath = Path.GetFullPath(f);
			project.AddOtherLDFlags(string.Format("-force_load \\\"{0}\\\"", fullPath));
		}

		// add frameworks
		PBXGroup frameworkGroup = project.GetGroup( "Frameworks" );
		foreach (var kvp in Frameworks) {
			var filename = kvp.Key;
			var frameworkPath = Path.Combine("System/Library/Frameworks", filename);
			project.AddFile(frameworkPath, frameworkGroup, "SDKROOT", true, kvp.Value == WEAK);
		}

		project.Save();
	}
}
