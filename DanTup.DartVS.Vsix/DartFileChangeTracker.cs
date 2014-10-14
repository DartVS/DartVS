using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using File = System.IO.File;

namespace DanTup.DartVS
{
	[Export(typeof(IVsTextViewCreationListener))]
	[ContentType("Dart")]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
	internal class DartFileChangeTracker : IVsTextViewCreationListener, IObserver<ITextSnapshot>
	{
		private IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
		private DartVsAnalysisService analysisService;

		[ImportingConstructor]
		public DartFileChangeTracker(IVsEditorAdaptersFactoryService editorAdaptersFactoryService, DartVsAnalysisService analysisService)
		{
			this.editorAdaptersFactoryService = editorAdaptersFactoryService;
			this.analysisService = analysisService;
		}

		public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			ITextView textView = editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
			if (textView == null)
				return;

			ITextBuffer documentBuffer = textView.TextDataModel.DocumentBuffer;
			if (!documentBuffer.ContentType.IsOfType("Dart"))
				return;

			documentBuffer.Properties.GetOrCreateSingletonProperty(typeof(DartFileChangeTracker), () => CreateBufferChangeListener(documentBuffer));
		}

		private IDisposable CreateBufferChangeListener(ITextBuffer textBuffer)
		{
			var postChanged = Observable.FromEventPattern(e => textBuffer.PostChanged += e, e => textBuffer.PostChanged -= e);
			return postChanged.Throttle(TimeSpan.FromMilliseconds(100)).Select(_ => textBuffer.CurrentSnapshot).Subscribe(this);
		}

		void IObserver<ITextSnapshot>.OnNext(ITextSnapshot value)
		{
			ITextBuffer textBuffer = value.TextBuffer;
			ITextDocument textDocument;
			if (!textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument) || textDocument == null)
				return;

			string path = textDocument.FilePath;
			if (!File.Exists(path))
				return;

			// TODO: Optimise this to use ChangeContentOverlay on subsequent updates.
			string fileContents = value.GetText();
			var change = new AddContentOverlay() { Type = "add", Content = fileContents };
			analysisService.UpdateContent(path, change);
		}

		void IObserver<ITextSnapshot>.OnError(Exception error)
		{
		}

		void IObserver<ITextSnapshot>.OnCompleted()
		{
		}
	}
}
