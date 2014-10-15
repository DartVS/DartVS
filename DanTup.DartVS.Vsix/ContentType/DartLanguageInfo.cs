using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartLanguageInfo : IVsLanguageInfo
	{
		ITextDocumentFactoryService textDocumentFactory;
		IVsEditorAdaptersFactoryService editorAdapterFactory;
		DartAnalysisServiceFactory analysisServiceFactory;

		public DartLanguageInfo(ITextDocumentFactoryService textDocumentFactory, IVsEditorAdaptersFactoryService editorAdapterFactory, DartAnalysisServiceFactory analysisServiceFactory)
		{
			this.textDocumentFactory = textDocumentFactory;
			this.editorAdapterFactory = editorAdapterFactory;
			this.analysisServiceFactory = analysisServiceFactory;
		}

		public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
		{
			ppCodeWinMgr = new DartCodeWindowManager(textDocumentFactory, editorAdapterFactory, pCodeWin, analysisServiceFactory);
			return VSConstants.S_OK;
		}

		public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
		{
			ppColorizer = null;
			return VSConstants.E_NOTIMPL;
		}

		public int GetFileExtensions(out string pbstrExtensions)
		{
			pbstrExtensions = DartConstants.FileExtension;
			return VSConstants.S_OK;
		}

		public int GetLanguageName(out string bstrName)
		{
			bstrName = DartConstants.LanguageName;
			return VSConstants.S_OK;
		}
	}
}
