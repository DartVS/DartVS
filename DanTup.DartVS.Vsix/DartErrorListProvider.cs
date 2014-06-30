using System;
using System.IO;
using System.Linq;
using DanTup.DartAnalysis;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	internal class DartErrorListProvider
	{
		DTE dte;
		ErrorListProvider errorProvider;

		internal DartErrorListProvider(DTE dte, DartPackage package)
		{
			this.dte = dte;
			errorProvider = new ErrorListProvider(package);
		}

		internal void UpdateErrors(AnalysisErrorsNotification errorNotification)
		{
			errorProvider.SuspendRefresh();
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
			errorProvider.ResumeRefresh();
			errorProvider.Show();
			errorProvider.ForceShowErrors();
		}

		void RemoveErrorsForFile(string path)
		{
			var staleTasks = errorProvider.Tasks.OfType<ErrorTask>().Where(t => t.Document == path).ToArray();
			foreach (var staleTask in staleTasks)
				errorProvider.Tasks.Remove(staleTask);
		}

		private ErrorTask CreateErrorTask(AnalysisError analysisError)
		{
			var lineInformaion = GetLineInformation(analysisError.File, analysisError.Offset);

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
				Line = lineInformaion.Line - 1, // Line appears to be 0-based! :-(
				Column = lineInformaion.Column
			};
		}

		/// <summary>
		/// Gets line information from an offset. The DartAnalysis service always returns offset/lengths, not lines/cols.
		/// </summary>
		LineInformation GetLineInformation(string path, int offset)
		{
			// Figure out if this file is open; as the contents might be different in the buffer to on disk.
			var openDocument = dte.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == path);

			// Read the contents from the buffer or from disk, depending on whether the file is open.
			string fileContents = null;
			if (openDocument != null)
			{
				var doc = openDocument.Object("") as TextDocument;
				fileContents = doc.CreateEditPoint().GetText(doc.EndPoint);
			}
			else
				fileContents = File.ReadAllText(path);

			// Walk through the file counting the characters in each line until we get to the offset.
			// TODO: Do we handle the count the same as Google's service for Unicode etc.? Needs some testing...
			var lines = fileContents.Split('\n');
			var totalCharacters = 0;
			for (int l = 0; l < lines.Length; l++)
			{
				// If this line ends prior to the offset, add the length and continue.
				if (totalCharacters + lines[l].Length < offset)
				{
					totalCharacters += lines[l].Length;
					continue;
				}
				// If the offset is within this line, return the information.
				else if (totalCharacters + lines[l].Length >= offset)
				{
					// Lines are 1-based!
					return new LineInformation(l + 1, offset - totalCharacters);
				}
				else
				{
					// Something has gone wrong :(
					return new LineInformation(1, 1);
				}
			}

			// Something has also gone wrong :(
			return new LineInformation(1, 1);
		}

		class LineInformation
		{
			public int Line { get; private set; }
			public int Column { get; private set; }

			public LineInformation(int line, int column)
			{
				this.Line = line;
				this.Column = column;
			}
		}
	}
}
