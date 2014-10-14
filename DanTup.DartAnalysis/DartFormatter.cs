using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Temporary SDK-based formatter, until the Analysis Service provides this functionality.
	/// </summary>
	public class DartFormatter : IDisposable
	{
		readonly string sdkFolder;

		public DartFormatter(string sdkFolder)
		{
			this.sdkFolder = sdkFolder;
		}

		public string FormatText(string text)
		{
			var tempFilename = string.Format("{0}.dart", Path.GetRandomFileName());
			var tempFileLocation = Path.Combine(Path.GetTempPath(), tempFilename);

			try
			{
				File.WriteAllText(tempFileLocation, text);

				var output = new StringBuilder();
				var errors = new StringBuilder();
				var didError = false;
				using (var formatter = new StdIOService(Path.Combine(sdkFolder, @"bin\dartfmt.bat"), tempFileLocation, msg => output.AppendLine(msg), err => { if (err != null) { errors.AppendLine(err); didError = true; } }))
					formatter.WaitForExit(); // Wait up to 30 seconds...

				if (didError)
				{
					Trace.WriteLine(errors.ToString());
					return text;
				}
				else
					return output.ToString().TrimEnd() + "\r\n"; // Always have a single trailing newline :)
			}
			finally
			{
				try
				{
					File.Delete(tempFileLocation);
				}
				catch { }
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
