using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Threading;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartGoToDefinition : IOleCommandTarget
	{
		IVsTextView textViewAdapter;
		IWpfTextView textView;
		ITextDocument textDocument;
		IOleCommandTarget nextCommandTarget;
		DartAnalysisService analysisService;

		AnalysisNavigationRegion[] navigationRegions = new AnalysisNavigationRegion[0];

		public DartGoToDefinition(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisService analysisService)
		{
			this.textViewAdapter = textViewAdapter;
			this.textView = textView;
			this.analysisService = analysisService;
			textDocumentFactory.TryGetTextDocument(textView.TextBuffer, out this.textDocument);

			Dispatcher.CurrentDispatcher.InvokeAsync(() =>
			{
				// Add the target later to make sure it makes it in before other command handlers
				ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandTarget));
			}, DispatcherPriority.ApplicationIdle);

			// Subscribe to outline updates for this file
			this.analysisService.AnalysisNavigationNotification.Where(en => en.File == textDocument.FilePath).Subscribe(UpdateNavigationData);
		}

		void UpdateNavigationData(AnalysisNavigationNotification notification)
		{
			navigationRegions = notification.Regions.ToArray();
		}

		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID && nCmdID == Convert.ToUInt32(VSConstants.VSStd97CmdID.GotoDefn, CultureInfo.InvariantCulture))
			{
				var offset = textView.Caret.Position.BufferPosition.Position;
				var navigationRegion = navigationRegions.FirstOrDefault(r => r.Offset <= offset && r.Offset + r.Length >= offset);

				if (navigationRegion.Targets != null && navigationRegion.Targets.Any())
				{
					// TODO: Show user if there are multiple!
					var target = navigationRegion.Targets.First();
					var file = target.Location.File;
					var position = target.Location.Offset;
					
					Helpers.OpenFileInPreviewTab(file);
					Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
					{
						try
						{
							IWpfTextView view = Helpers.GetCurentTextView();
							ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;
							view.Caret.MoveTo(new SnapshotPoint(snapshot, position));
							view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(snapshot, position, 1), EnsureSpanVisibleOptions.AlwaysCenter);
						}
						catch
						{ }

					}), DispatcherPriority.ApplicationIdle, null);
				}
				else
				{
					// TODO: Alert user!
				}
			}

			return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (pguidCmdGroup != typeof(VSConstants.VSStd97CmdID).GUID)
				return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

			for (int i = 0; i < cCmds; i++)
			{
				if (prgCmds[i].cmdID == Convert.ToUInt32(VSConstants.VSStd97CmdID.GotoDefn, CultureInfo.InvariantCulture))
				{
					prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
					return VSConstants.S_OK;
				}
			}

			return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}
	}
}
