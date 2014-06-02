using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	/// <summary>
	/// Analyzes a dart file and updates the Visual Studio error list with the results.
	/// </summary>
	static class DartAnalyzerErrorService
	{
		public static void Parse(ErrorListProvider errorListProvider, string path)
		{
			lock (errorListProvider)
			{
				// Remove all tasks for the current file, since they're likely out-of-date
				// if the user has just saved
				RemoveStaleErrors(errorListProvider, path);

				// Show a message that we're executing; since this can be slow. Otherwise we'd
				// have to leave known-stale errors, or empty list (which suggests no errors).
				errorListProvider.Tasks.Add(new ErrorTask
				{
					ErrorCategory = TaskErrorCategory.Message,
					Document = path,
					Text = "Executing DartAnalyzer, please wait..."
				});
			}


			IEnumerable<ErrorTask> errors;
			try
			{
				errors = new DartAnalyzer().Analyze(path);
			}
			catch (Exception ex)
			{
				// Update the error list eith the new errors
				lock (errorListProvider)
				{
					RemoveStaleErrors(errorListProvider, path);
					errorListProvider.Tasks.Add(new ErrorTask(new Exception("Unable to execute DartAnalzyer: " + ex.ToString())));
					errorListProvider.Show();
					return;
				}
			}

			// Update the error list eith the new errors
			lock (errorListProvider)
			{
				RemoveStaleErrors(errorListProvider, path);
				foreach (var error in errors)
				{
					error.Navigate += (s, e) => errorListProvider.Navigate(error, new Guid(EnvDTE.Constants.vsViewKindCode));
					errorListProvider.Tasks.Add(error);
				}
				errorListProvider.Show();
			}
		}

		private static void RemoveStaleErrors(ErrorListProvider errorListProvider, string path)
		{
			var staleTasks = errorListProvider.Tasks.OfType<ErrorTask>().Where(t => t.Document == path).ToArray(); // Force execution so we don't modify while enumerating
			foreach (var staleTask in staleTasks)
				errorListProvider.Tasks.Remove(staleTask);
		}
	}
}
