using System;
using System.Globalization;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartFormatDocument : IOleCommandTarget
	{
		IVsTextView textViewAdapter;
		IWpfTextView textView;
		ITextDocument textDocument;
		IOleCommandTarget nextCommandTarget;

		public DartFormatDocument(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView)
		{
			this.textViewAdapter = textViewAdapter;
			this.textView = textView;
			textDocumentFactory.TryGetTextDocument(textView.TextBuffer, out this.textDocument);

			Dispatcher.CurrentDispatcher.InvokeAsync(() =>
			{
				// Add the target later to make sure it makes it in before other command handlers.
				ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandTarget));
			}, DispatcherPriority.ApplicationIdle);
		}

		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID && nCmdID == Convert.ToUInt32(VSConstants.VSStd2KCmdID.FORMATDOCUMENT, CultureInfo.InvariantCulture))
			{
				// TODO: Implement me
			}

			return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (pguidCmdGroup != typeof(VSConstants.VSStd2KCmdID).GUID)
				return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

			for (int i = 0; i < cCmds; i++)
			{
				if (prgCmds[i].cmdID == Convert.ToUInt32(VSConstants.VSStd2KCmdID.FORMATDOCUMENT, CultureInfo.InvariantCulture))
				{
					prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
					return VSConstants.S_OK;
				}
			}

			return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}
	}
}
