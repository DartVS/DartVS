using System;
using System.Linq;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	internal class DartErrorListProvider
	{
		ErrorListProvider errorProvider;

		internal DartErrorListProvider(DartPackage package)
		{
			errorProvider = new ErrorListProvider(package);
		}

		internal void UpdateErrors(AnalysisErrorsNotification errorNotification)
		{
			RemoveErrorsForFile(errorNotification.File);

			var errorTasks = errorNotification.Errors.Select(CreateErrorTask);

			foreach (var error in errorTasks)
			{
				error.Navigate += (s, e) =>
				{
					error.Line++; // Line number seems 0-based in most places, but Navigate didn't get the memo :(
					errorProvider.Navigate(error, new Guid(EnvDTE.Constants.vsViewKindCode));
					error.Line--;
				};
				errorProvider.Tasks.Add(error);
			}
			errorProvider.Show();
			errorProvider.ForceShowErrors();
		}

		private ErrorTask CreateErrorTask(AnalysisError analysisError)
		{
			return new ErrorTask
			{
				// TODO: This is bogus; they don't start with this anymore...
				ErrorCategory =
					analysisError.Message.StartsWith("hint") ? TaskErrorCategory.Message
					: analysisError.Message.StartsWith("warning") ? TaskErrorCategory.Warning
					: TaskErrorCategory.Error,
				Text = analysisError.Message,
				Document = analysisError.File,
				// TODO: Figure this out (we have offset + length!)
				Line = 1 - 1, // Line appears to be 0-based! :-(
				Column = 1,
			};
		}

		void RemoveErrorsForFile(string path)
		{
			var staleTasks = errorProvider.Tasks.OfType<ErrorTask>().Where(t => t.Document == path).ToArray();
			foreach (var staleTask in staleTasks)
				errorProvider.Tasks.Remove(staleTask);
		}
	}
}
