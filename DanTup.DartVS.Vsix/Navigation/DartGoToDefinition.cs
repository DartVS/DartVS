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
	class DartGoToDefinition : DartOleCommandTarget<VSConstants.VSStd97CmdID>
	{
		IDisposable subscription;
		AnalysisNavigationRegion[] navigationRegions = new AnalysisNavigationRegion[0];

		public DartGoToDefinition(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisService analysisService)
			: base(textDocumentFactory, textViewAdapter, textView, analysisService, VSConstants.VSStd97CmdID.GotoDefn)
		{
			// Subscribe to outline updates for this file.
			subscription = this.analysisService.AnalysisNavigationNotification.Where(en => en.File == textDocument.FilePath).Subscribe(UpdateNavigationData);
		}

		void UpdateNavigationData(AnalysisNavigationEvent notification)
		{
			navigationRegions = notification.Regions.ToArray();
		}

		protected override void Exec()
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
	}
}
