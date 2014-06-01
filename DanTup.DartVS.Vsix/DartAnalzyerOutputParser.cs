using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	/// <summary>
	/// Parses the output of DartAnalyzer into Visual Studio ErrorTasks.
	/// </summary>
	class DartAnalzyerOutputParser
	{
		static readonly Regex dartAnazlyserOutputLine = new Regex(@"^\[(\w+)\] (.+) \((.+), line (\d+), col (\d+)\)$", RegexOptions.Compiled);

		public IEnumerable<ErrorTask> Parse(string output)
		{
			// Analyzing [M:\Coding\TestApps\DartTests\Script.dart]...
			// No issues found

			// Analyzing [M:\Coding\TestApps\DartTests\Script.dart]...
			// [warning] Undefined name 'window2' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 2)
			// [hint] Unused import (M:\Coding\TestApps\DartTests\Script.dart, line 1, col 8)
			// 1 error found.

			// Analyzing [M:\Coding\TestApps\DartTests\Script.dart]...
			// [error] Expected to find ';' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 16)
			// [error] Expected to find ')' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 19)
			// [error] Expected to find ';' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 19)
			// [error] Expected an identifier (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 24)
			// [error] Expected to find ';' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 24)
			// [error] Unexpected token ',' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 24)
			// [error] Expected to find ';' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 26)
			// [error] Expected to find ';' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 31)
			// [error] Unterminated string literal (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 34)
			// [warning] Undefined name 'window2' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 2)
			// [warning] Undefined name 'Hello' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 19)
			// [warning] Undefined name 'world' (M:\Coding\TestApps\DartTests\Script.dart, line 4, col 26)
			// [hint] Unused import (M:\Coding\TestApps\DartTests\Script.dart, line 1, col 8)
			// 12 errors found.

			Func<string, bool> isIssue = l => l.StartsWith("[error]") || l.StartsWith("[warning]") || l.StartsWith("[hint]");
			Func<string, ErrorTask> createTask = l =>
			{
				var match = dartAnazlyserOutputLine.Match(l);

				return new ErrorTask
				{
					ErrorCategory =
						match.Groups[1].Value == "hint"
						? TaskErrorCategory.Message
						: match.Groups[1].Value == "warning"
							? TaskErrorCategory.Warning
							: TaskErrorCategory.Error,
					Text = match.Groups[2].Value,
					Document = match.Groups[3].Value,
					Line = int.Parse(match.Groups[4].Value),
					Column = int.Parse(match.Groups[5].Value),
				};
			};

			return output
				.Split('\n')
				.Select(l => l.Trim())
				.Where(isIssue)
				.Select(createTask);
		}
	}
}
