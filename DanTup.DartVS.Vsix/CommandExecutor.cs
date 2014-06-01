using System;
using System.Diagnostics;
using System.IO;

namespace DanTup.DartVS
{
	/// <summary>
	/// Executes an external command and collects output. Anything written to STDERR will cause an exception to be thrown with the data,
	/// </summary>
	class CommandExecutor
	{
		public string ExecuteCommand(string file, string args)
		{
			var startInfo = new ProcessStartInfo(file, args)
			{
				WorkingDirectory = Path.GetDirectoryName(file),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			using (var proc = new Process { StartInfo = startInfo })
			{
				proc.Start();

				var output = proc.StandardOutput.ReadToEnd();
				var error = proc.StandardError.ReadToEnd();
				proc.WaitForExit();

				if (!string.IsNullOrEmpty(error))
					throw new Exception(error);

				return output;
			}
		}
	}
}
