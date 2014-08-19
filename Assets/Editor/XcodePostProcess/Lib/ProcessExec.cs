using System;
using System.Diagnostics;
using System.IO;

namespace XcodePostProcessLib
{
	public static class ProcessExec
	{
		/// <summary>
		/// Runs the specified executable (fileName) with arguments (args) and returns
		/// the contents of stdout.
		/// </summary>
		public static string Exec(string fileName, string args = null)
		{
			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.FileName = fileName;
			proc.StartInfo.Arguments = args;
			proc.Start();
			
			string output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit();
			
			return output;
		}
	}
}