using DanTup.DartAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartFormatDocument : DartOleCommandTarget<VSConstants.VSStd2KCmdID>
	{
		public DartFormatDocument(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisService analysisService)
			: base(textDocumentFactory, textViewAdapter, textView, analysisService, VSConstants.VSStd2KCmdID.FORMATDOCUMENT)
		{
		}

		protected override void Exec()
		{
			// Get the current state of the document (we can't format on-disk, as it might not have been saved).
			var fileContents = textView.TextSnapshot.GetText();
			var caretLine = textView.TextSnapshot.GetLineNumberFromPosition(textView.Caret.Position.BufferPosition.Position);

			// Call the formatter.
			string formattedFileContents;
			using (var formatter = new DartFormatter(DartAnalysisService.SdkPath))
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
