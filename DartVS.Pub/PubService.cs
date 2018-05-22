using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

namespace DartVS.Pub
{
	/// <summary>
	/// Wrapper around command-line Pub application.
	/// </summary>
	[Export]
	public class PubService
	{
		/// <summary>
		/// Runs "pub get" in the provided directory.
		/// </summary>
		/// <param name="projectRoot">Thedirectory to run "pub get" from.</param>
		/// <param name="outputHandler">A function to receive any output from pub (STDOUT/STDERR).</param>
		/// <returns>The exit code from pub.</returns>
		public async Task<bool> GetAsync(string projectRoot, Action<string> outputHandler)
		{
			var sdkFolder = await DartSdk.GetSdkPathAsync();

			using (var proc = new StdIOService(projectRoot, Path.Combine(sdkFolder, @"bin\pub.bat"), "get", outputHandler, outputHandler))
				return proc.WaitForExit() >= 0;
		}
	}
}
