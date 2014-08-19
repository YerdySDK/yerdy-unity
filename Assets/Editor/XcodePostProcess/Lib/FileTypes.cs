using System;
using System.Collections.Generic;
using System.IO;

namespace XcodePostProcessLib
{

	/// <summary>
	/// Supported file types. Should include all the types we probably need
	/// TODO: Support all Xcode file types
	/// </summary>
	public enum FileType
	{
		Unknown, 
		
		SourceHeader,
		SourceC,
		SourceObjC,
		SourceCpp,
		SourceObjCCpp,
		SourceAsm,
		
		StaticLibrary,	// archive (.a)
		DynamicLibrary,	// .dylib
		Framework,		// .framework
		App,			// .app
		
		ImageBmp,
		ImageGif,
		ImageIcon,			// .icns
		ImageJpeg,
		ImageMicrosoftIcon,	// .ico
		ImagePict,
		ImagePng,
		ImageTiff,
		
		PlistBinary,		// .plist
		PlistText,			// .plist
		PlistXml,			// .plist
		Xml,
		
		Txt,
	}
	
	/// <summary>
	/// Supported Xcode build phases
	/// </summary>
	public enum FileBuildPhase
	{
		Unknown,
		Sources,
		Frameworks,
		Resources,
	}
	
	public static class FileTypes
	{
		/// <summary>
		/// Given a path/file/ext, returns the 
		/// </summary>
		public static FileType GetFileType(string pathOrFileOrExt)
		{
			string ext;
			if (Path.HasExtension(pathOrFileOrExt))
				ext = Path.GetExtension(pathOrFileOrExt);
			else
				ext = pathOrFileOrExt;
			ext = ext.Trim('.');
			
			switch (ext)
			{
			case "h" :
			case "hpp": 
				return FileType.SourceHeader;
			case "c": return FileType.SourceC;
			case "m": return FileType.SourceObjC;
			case "cpp":
			case "cxx":
				return FileType.SourceCpp;
			case "mm": return FileType.SourceObjCCpp;
			case "s": return FileType.SourceAsm;
			case "a": return FileType.StaticLibrary;
			case "dylib": return FileType.DynamicLibrary;
			case "framework": return FileType.Framework;
			case "app": return FileType.App;
			case "bmp": return FileType.ImageBmp;
			case "gif": return FileType.ImageGif;
			case "icns": return FileType.ImageIcon;
			case "jpg":
			case "jpeg":
				return FileType.ImageJpeg;
			case "ico": return FileType.ImageMicrosoftIcon;
			case "pict": return FileType.ImagePict;
			case "png": return FileType.ImagePng;
			case "tif":
			case "tiff":
				return FileType.ImageTiff;
				
			// 9 times out of 10 it'll be an XML plist, and if not Xcode will figure it out anyways
			case "plist":
				return FileType.PlistXml;
			case "xml": return FileType.Xml;
			case "txt": return FileType.Txt;
			}
			
			return FileType.Unknown;
		}
		
		/// <summary>
		/// Returns a string to be used for the value of "lastKnownFileType" in the .pbxproj
		/// If an identifier can't be determined, returns "unknown" - Xcode will figure it out for us
		/// </summary>
		public static string GetXcodeIdentifier(FileType fileType)
		{
			switch (fileType)
			{
			case FileType.Unknown: return "unknown";
				
			case FileType.SourceHeader: return "sourcecode.c.h";
			case FileType.SourceC: return "sourcecode.c";
			case FileType.SourceObjC: return "sourcecode.c.objc";
			case FileType.SourceCpp: return "sourcecode.cpp.cpp";
			case FileType.SourceObjCCpp: return "sourcecode.cpp.objcpp";
			case FileType.SourceAsm: return "sourcecode.asm";
			
			case FileType.StaticLibrary: return "archive.ar";
			case FileType.DynamicLibrary: return "compiled.mach-o.dylib";
			case FileType.Framework: return "wrapper.framework";
			case FileType.App: return "wrapper.application";
			
			case FileType.ImageBmp: return "image.bmp";
			case FileType.ImageGif: return "image.gif";
			case FileType.ImageIcon: return "image.icns";
			case FileType.ImageJpeg: return "image.jpeg";
			case FileType.ImageMicrosoftIcon: return "image.ico";
			case FileType.ImagePict: return "image.pict";
			case FileType.ImagePng: return "image.png";
			case FileType.ImageTiff: return "image.tiff";
			
			case FileType.PlistBinary: return "file.bplist";
			case FileType.PlistText: return "text.plist";
			case FileType.PlistXml: return "text.plist.xml";
			case FileType.Xml: return "text.xml";
				
			case FileType.Txt: return "text";
			}
			
			return "unknown";
		}
		
		/// <summary>
		/// Gets the build phase for a FileType. Returns Unknown when a build phase can't be
		/// determined
		/// </summary>
		public static FileBuildPhase GetBuildPhase(FileType fileType)
		{
			switch (fileType)
			{
			case FileType.Unknown: return FileBuildPhase.Unknown;
				
			case FileType.SourceHeader:
				return FileBuildPhase.Unknown;

			case FileType.SourceC:
			case FileType.SourceObjC:
			case FileType.SourceCpp:
			case FileType.SourceObjCCpp:
			case FileType.SourceAsm:
				return FileBuildPhase.Sources;
			
			case FileType.StaticLibrary:
			case FileType.DynamicLibrary:
			case FileType.Framework:
				return FileBuildPhase.Frameworks;
			case FileType.App:
				return FileBuildPhase.Unknown;
			
			case FileType.ImageBmp:
			case FileType.ImageGif:
			case FileType.ImageIcon:
			case FileType.ImageJpeg:
			case FileType.ImageMicrosoftIcon:
			case FileType.ImagePict:
			case FileType.ImagePng:
			case FileType.ImageTiff:
				return FileBuildPhase.Resources;
			
			case FileType.PlistBinary:
			case FileType.PlistText:
			case FileType.PlistXml:
			case FileType.Xml:
				return FileBuildPhase.Resources;
				
			case FileType.Txt:
				return FileBuildPhase.Resources;
			}
			
			return FileBuildPhase.Unknown;
		}
		
		/// <summary>
		/// Gets the human-friendly string for a build phase (used in pbxproj comments like:
		/// /* File.mm in Sources */)
		/// </summary>
		public static string GetHumanizedBuildPhase(FileBuildPhase phase)
		{
			return phase.ToString();
		}
	}
	
}