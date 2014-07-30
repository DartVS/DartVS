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
			// TODO: Implement me
		}
	}
}
