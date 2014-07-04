using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartCodeWindowManager : IVsCodeWindowManager
	{
		public DartCodeWindowManager()
		{
		}

		public int AddAdornments()
		{
			return VSConstants.S_OK;
		}

		public int OnNewView(IVsTextView pView)
		{
			return VSConstants.S_OK;
		}

		public int RemoveAdornments()
		{
			return VSConstants.S_OK;
		}
	}
}
