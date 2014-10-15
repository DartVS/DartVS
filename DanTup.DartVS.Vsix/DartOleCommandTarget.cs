using System;
using System.Globalization;
using System.Linq;
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
		protected DartAnalysisServiceFactory analysisServiceFactory;

		uint[] commandIDs;

		public DartOleCommandTarget(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisServiceFactory analysisServiceFactory, params T[] commandIDs)
		{
			this.textViewAdapter = textViewAdapter;
			this.textView = textView;
			this.analysisServiceFactory = analysisServiceFactory;
			this.commandIDs = commandIDs.Select(commandID => Convert.ToUInt32(commandID, CultureInfo.InvariantCulture)).ToArray();

			textDocumentFactory.TryGetTextDocument(textView.TextBuffer, out this.textDocument);

			Dispatcher.CurrentDispatcher.InvokeAsync(() =>
			{
				// Add the target later to make sure it makes it in before other command handlers.
				ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandTarget));
			}, DispatcherPriority.ApplicationIdle);
		}

		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (pguidCmdGroup == typeof(T).GUID && commandIDs.Contains(nCmdID))
			{
				this.Exec(nCmdID, pvaIn);
			}

			return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		protected abstract void Exec(uint nCmdID, IntPtr pvaIn);

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (pguidCmdGroup != typeof(T).GUID)
				return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

			for (int i = 0; i < cCmds; i++)
			{
				if (commandIDs.Contains(prgCmds[i].cmdID))
				{
					prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
					return VSConstants.S_OK;
				}
			}

			return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}
	}
}
