using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartLanguageInfo : IVsLanguageInfo
	{
		public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
		{
			ppCodeWinMgr = new DartCodeWindowManager();
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
