using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Editor;
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
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		IVsEditorAdaptersFactoryService editorAdaptersFactoryService = null;

		[Import]
		DartAnalysisService analysisService = null;

		public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			var textView = editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

			textView.Properties.GetOrCreateSingletonProperty<DartGoToDefinition>(() => new DartGoToDefinition(textDocumentFactory, textViewAdapter, textView, analysisService));
			textView.Properties.GetOrCreateSingletonProperty<DartFormatDocument>(() => new DartFormatDocument(textDocumentFactory, textViewAdapter, textView));
		}
	}
}
