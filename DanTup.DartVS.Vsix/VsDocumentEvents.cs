using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DanTup.DartVS
{
	/// <summary>
	/// Helper to hide away some of the not-so-elegant VS API for catching Document events.
	/// </summary>
	sealed class VsDocumentEvents : IVsRunningDocTableEvents, IDisposable
	{
		IVsRunningDocumentTable documentService;
		private uint cookie;
		public event EventHandler<string> FileSaved;

		public VsDocumentEvents()
		{
			documentService = Package.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
			if (documentService != null)
				documentService.AdviseRunningDocTableEvents(this, out cookie);
		}

		public void Dispose()
		{
			if (documentService != null && cookie != 0)
				documentService.UnadviseRunningDocTableEvents(cookie);
		}

		void OnFileSaved(string filename)
		{
			var handler = FileSaved;
			if (handler != null)
				handler(this, filename);
		}

		#region IVsRunningDocTableEvents Events

		public int OnAfterSave(uint docCookie)
		{
			string filename;

			uint pgrfRDTFlags, pdwReadLocks, pdwEditLocks;
			IVsHierarchy ppHier;
			uint pitemid;
			IntPtr ppunkDocData;
			documentService.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out filename, out ppHier, out pitemid, out ppunkDocData);

			OnFileSaved(filename);

			return VSConstants.S_OK;
		}

		#region Events we don't care about

		public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#endregion
	}
}
