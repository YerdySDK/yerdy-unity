using System;

namespace XcodePostProcessLib
{
	/// <summary>
	/// Wraps editing the Xcode project file, tracking whether or not modifications
	/// were successful.
	/// </summary>
	public class ProjectSourceEditor
	{
		private string contents;

		public ProjectSourceEditor(string fileContents)
		{
			contents = fileContents;
		}

		/// <summary>
		/// Whether or not any modifications failed
		/// </summary>
		public bool IsValid
		{
			get { return contents != null; }
		}

		/// <summary>
		/// Gets the modified file contents.  If a modification failed, returns null
		/// </summary>
		public string Contents
		{
			get { return contents; }
		}

		/// <summary>
		/// Modify the contents of the file.  On failure, your 'func' should return null
		/// </summary>
		public void Modify(Func<string, string> func)
		{
			try
			{
				if (contents != null) {
					contents = func(contents);
				}
			}
			catch (Exception ex)
			{
				if (ex is OutOfMemoryException)
				{
					// Depending on the user's machine, we *may* see an
					// OutOfMemoryException while running the regexes to
					// modify the project file.
					//
					// We'll simply silently ignore these (since it'll fall
					// back on modifying the project via a plist)
					contents = null;
				}
				else
				{
					UnityEngine.Debug.LogException(ex);
					throw;
				}
			}
		}
	}
}