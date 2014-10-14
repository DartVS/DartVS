using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(IVsTextViewCreationListener))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	class DartViewCreationListener : IVsTextViewCreationListener
	{
		[Import]
		SVsServiceProvider serviceProvider = null;

		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		IVsEditorAdaptersFactoryService editorAdaptersFactoryService = null;

		[Import]
		ICompletionBroker completionBroker = null;

		[Import]
		DartAnalysisServiceFactory analysisServiceFactory = null;

		public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			var textView = editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

			textView.Properties.GetOrCreateSingletonProperty<DartGoToDefinition>(() => new DartGoToDefinition(serviceProvider, textDocumentFactory, textViewAdapter, textView, analysisServiceFactory));
			textView.Properties.GetOrCreateSingletonProperty<DartFormatDocument>(() => new DartFormatDocument(textDocumentFactory, textViewAdapter, textView, analysisServiceFactory));
			textView.Properties.GetOrCreateSingletonProperty<CompletionController>(() => new CompletionController(textDocumentFactory, textViewAdapter, textView, completionBroker, analysisServiceFactory));
		}
	}
}
