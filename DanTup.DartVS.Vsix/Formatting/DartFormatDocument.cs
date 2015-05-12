using System;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
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

			// Call the formatter.
			// TODO: Switch to service...
			string formattedFileContents;
			using (var formatter = new DartFormatter(analysisService.Result.SdkFolder))
				formattedFileContents = formatter.FormatText(fileContents);

			// Create a span that is the entire document, since we're going to replace it.
			var entireDocumentSpan = new Span(0, textView.TextSnapshot.TextBuffer.CurrentSnapshot.Length);

			// Replace the document with the formatted version.
			textView.TextSnapshot.TextBuffer.Replace(entireDocumentSpan, formattedFileContents);

			// Set the caret back to the correct line if it isn't a hihger line number than we now have.
			if (caretLine < textView.TextSnapshot.LineCount)
				textView.Caret.MoveTo(textView.TextSnapshot.GetLineFromLineNumber(caretLine).Start);
		}
	}
}
