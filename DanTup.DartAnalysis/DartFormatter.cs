using System.IO;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Temporary SDK-based formatter, until the Analysis Service provides this functionality.
	/// </summary>
	public class DartFormatter
	{
		public string FormatText(string text)
		{
			var tempFilename = string.Format("{0}.dart", Path.GetRandomFileName());
			var tempFileLocation = Path.Combine(Path.GetTempPath(), tempFilename);

			try
			{
				File.WriteAllText(tempFileLocation, text);

				// TODO: Call SDK formatter
				return text;
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
	}
}
