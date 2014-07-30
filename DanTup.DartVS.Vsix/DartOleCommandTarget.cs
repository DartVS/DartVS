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
	abstract class DartOleCommandTarget<T> : IOleCommandTarget where T : struct, IConvertible
	{
		protected IVsTextView textViewAdapter;
		protected IWpfTextView textView;
		protected ITextDocument textDocument;
		protected IOleCommandTarget nextCommandTarget;
		DartAnalysisService analysisService;

		T commandID;

		public DartOleCommandTarget(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisService analysisService, T commandID)
		{
			this.textViewAdapter = textViewAdapter;
			this.textView = textView;
			this.analysisService = analysisService;
			this.commandID = commandID;

			textDocumentFactory.TryGetTextDocument(textView.TextBuffer, out this.textDocument);

			Dispatcher.CurrentDispatcher.InvokeAsync(() =>
			{
				// Add the target later to make sure it makes it in before other command handlers.
				ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandTarget));
			}, DispatcherPriority.ApplicationIdle);
		}

		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (pguidCmdGroup == typeof(T).GUID && nCmdID == Convert.ToUInt32(commandID, CultureInfo.InvariantCulture))
			{
				this.Exec();
			}

			return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		protected abstract void Exec();

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (pguidCmdGroup != typeof(T).GUID)
				return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

			for (int i = 0; i < cCmds; i++)
			{
				if (prgCmds[i].cmdID == Convert.ToUInt32(commandID, CultureInfo.InvariantCulture))
				{
					prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
					return VSConstants.S_OK;
				}
			}

			return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}
	}
}
