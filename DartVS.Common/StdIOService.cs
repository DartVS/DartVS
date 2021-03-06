﻿using System;
using System.Diagnostics;
using System.Text;

namespace DartVS
{
	/// <summary>
	/// Wraps a process for two-way communication over STDIN/STDOUT.
	/// </summary>
	public class StdIOService : IDisposable
	{
		readonly Process process;
		
		/// <summary>
		/// Launches the provided process with the provided arguments and calls <paramref name="outputHandler"/> and
		/// <paramref name="errorHandler"/> as data is received on STDOUT and STDERR.
		/// </summary>
		/// <param name="workingDir">The directory to start the process in.</param>
		/// <param name="file">The executable file to start.</param>
		/// <param name="args">The arguments to pass to the executable file.</param>
		/// <param name="outputHandler">A handler to be called for data recieved from STDOUT.</param>
		/// <param name="errorHandler">A handler to be called for data recieved from STDERR.</param>
		public StdIOService(string workingDir, string file, string args, Action<string> outputHandler, Action<string> errorHandler)
		{
			var info = new ProcessStartInfo
			{
				WorkingDirectory = workingDir,
				FileName = file,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8,
			};

			process = Process.Start(info);

			process.OutputDataReceived += (sender, e) => outputHandler(e.Data);
			process.ErrorDataReceived += (sender, e) => errorHandler(e.Data);

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}

		/// <summary>
		/// Writes a line to STDIN of the wrapped process.
		/// </summary>
		/// <param name="value">The data to send to STDIN (newline is automatically appended).</param>
		public void WriteLine(string value)
		{
			if (process.HasExited)
				throw new Exception("Process has exited!");

			lock (process.StandardInput) // Can't find any info on whether WriteLine below is threadsafe, so letassume the worst
				process.StandardInput.WriteLine(value);
		}

		public int WaitForExit(int milliseconds = 0)
		{
			if (milliseconds == 0)
				process.WaitForExit();
			else
				process.WaitForExit(milliseconds);

			return process.ExitCode;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					process.Kill();
				}
				catch { }
				try
				{
					process.Close();
				}
				catch { }
			}
		}
	}
}
