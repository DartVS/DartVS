using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	// https://github.com/Sectoid/debian-dlr-languages/blob/5f1b292f62f2b5a2d92c5a564ff58816419af1da/Tools/IronStudio/IronPythonTools/IronPythonTools/Navigation/CodeWindowManager.cs
	class DartCodeWindowManager : IVsCodeWindowManager
	{
		IVsDropdownBarManager barManager;
		IWpfTextView wpfTextView;
		ITextDocument textDocument;
		DartAnalysisService analysisService;
		NavigationDropdown dropdown;

		public DartCodeWindowManager(ITextDocumentFactoryService textDocumentFactory, IVsEditorAdaptersFactoryService editorAdapterFactory, IVsCodeWindow codeWindow, DartAnalysisService analysisService)
		{
			this.barManager = ((IVsDropdownBarManager)codeWindow);
			this.analysisService = analysisService;

			// Figure out the filename (seriously; this is the best way?!).
			IVsTextView textView;
			codeWindow.GetPrimaryView(out textView);
			wpfTextView = editorAdapterFactory.GetWpfTextView(textView);
			textDocumentFactory.TryGetTextDocument(wpfTextView.TextBuffer, out this.textDocument);
		}

		public int AddAdornments()
		{
			dropdown = new NavigationDropdown(analysisService, textDocument.FilePath, wpfTextView);
			barManager.AddDropdownBar(2, dropdown);
			return VSConstants.S_OK;
		}

		public int OnNewView(IVsTextView pView)
		{
			return VSConstants.S_OK;
		}

		public int RemoveAdornments()
		{
			dropdown.Unregister();
			barManager.RemoveDropdownBar();
			return VSConstants.S_OK;
		}
	}
}
