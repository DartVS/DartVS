using System;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartFormatDocument : DartOleCommandTarget<VSConstants.VSStd2KCmdID>
	{
		readonly Task<DartAnalysisService> analysisService;

		public DartFormatDocument(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisServiceFactory analysisServiceFactory)
			: base(textDocumentFactory, textViewAdapter, textView, analysisServiceFactory, VSConstants.VSStd2KCmdID.FORMATDOCUMENT)
		{
			analysisService = analysisServiceFactory.GetAnalysisServiceAsync();
		}

		protected override void Exec(uint nCmdID, IntPtr pvaIn)
		{
			if (analysisService.Status != TaskStatus.RanToCompletion)
				throw new NotSupportedException("The analysis service is not available.");

			// Get the current state of the document (we can't format on-disk, as it might not have been saved).
			var fileContents = textView.TextSnapshot.GetText();
			var caretLine = textView.TextSnapshot.GetLineNumberFromPosition(textView.Caret.Position.BufferPosition.Position);

			analysisService
				.ContinueWith(service => service.Result.Format(textDocument.FilePath, textView.Caret.Position.BufferPosition.Position, 0))
				.Unwrap()
				.ContinueWith(UpdateContent, TaskScheduler.FromCurrentSynchronizationContext());
		}

		void UpdateContent(Task<EditFormatResponse> formatResponse)
		{
			// Replace the document with the formatted version.
			foreach (var edit in formatResponse.Result.Edits)
			{
				var editSpan = new Span(edit.Offset, edit.Length);
				textView.TextSnapshot.TextBuffer.Replace(editSpan, edit.Replacement);
			}

			var index = formatResponse.Result.SelectionOffset;

			textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, index));

			textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, index, 0), EnsureSpanVisibleOptions.AlwaysCenter);
		}

	}
}
