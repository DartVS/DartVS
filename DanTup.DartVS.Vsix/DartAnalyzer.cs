using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	/// <summary>
	/// Analyzes a dart file and reports back errors in the form of Visual Studio ErrorTasks.
	/// </summary>
	class DartAnalyzer
	{
		readonly CommandExecutor commandExecutor = new CommandExecutor();
		readonly DartAnalzyerOutputParser outputParser = new DartAnalzyerOutputParser();

		static bool hasReportedDartSdkMissing = false;

		public IEnumerable<ErrorTask> Analyze(string filename)
		{
			var dartAnalyzerPath = GetDartAnalyzerPath();

			if (!string.IsNullOrWhiteSpace(dartAnalyzerPath))
				return AnalyzeFile(dartAnalyzerPath, filename);
			else if (!hasReportedDartSdkMissing)
			{
				hasReportedDartSdkMissing = true;
				return new[] { new ErrorTask { Text = "The Dart SDK could not be found. Please set the DART_SDK environment variable to the SDK root." } };
			}
			else
				return Enumerable.Empty<ErrorTask>();
		}

		private static string GetDartAnalyzerPath()
		{
			// For some reason, if I don't pass User/Machine here, I seem to always get null! :(
			var sdkRoot = Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.User)
				?? Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Machine);
			if (sdkRoot == null)
				return null;

			var analyzer = Path.Combine(sdkRoot, @"bin\dartanalyzer.bat");
			if (File.Exists(analyzer))
				return analyzer;
			else
				return null;
		}

		private IEnumerable<ErrorTask> AnalyzeFile(string dartAnalyzerPath, string filename)
		{
			var args = string.Format("--fatal-warnings \"{0}\"", filename.Replace("\"", "\\\""));

			var commandOutput = commandExecutor.ExecuteCommand(dartAnalyzerPath, args);

			return outputParser.Parse(commandOutput);
		}
	}
}
