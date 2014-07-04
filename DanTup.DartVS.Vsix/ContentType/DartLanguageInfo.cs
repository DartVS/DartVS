using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;

namespace DanTup.DartVS
{
	class DartLanguageInfo : IVsLanguageInfo
	{
		ITextDocumentFactoryService textDocumentFactory;
		IVsEditorAdaptersFactoryService editorAdapterFactory;

		public DartLanguageInfo(ITextDocumentFactoryService textDocumentFactory, IVsEditorAdaptersFactoryService editorAdapterFactory)
		{
			this.textDocumentFactory = textDocumentFactory;
			this.editorAdapterFactory = editorAdapterFactory;
		}

		public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
		{
			ppCodeWinMgr = new DartCodeWindowManager(textDocumentFactory, editorAdapterFactory, pCodeWin);
			return VSConstants.S_OK;
		}

		public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
		{
			ppColorizer = null;
			return VSConstants.E_NOTIMPL;
		}

		public int GetFileExtensions(out string pbstrExtensions)
		{
			pbstrExtensions = ".dart";
			return VSConstants.S_OK;
		}

		public int GetLanguageName(out string bstrName)
		{
			bstrName = "Dart";
			return VSConstants.S_OK;
		}
	}
}
