using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class DartGoToDefinition : DartOleCommandTarget<VSConstants.VSStd97CmdID>
	{
		SVsServiceProvider serviceProvider;
		Task<IDisposable> subscription;
		AnalysisNavigationNotification navigationNotification = null;

		public DartGoToDefinition(SVsServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, DartAnalysisServiceFactory analysisServiceFactory)
			: base(textDocumentFactory, textViewAdapter, textView, analysisServiceFactory, VSConstants.VSStd97CmdID.GotoDefn)
		{
			this.serviceProvider = serviceProvider;

			// Subscribe to outline updates for this file.
			subscription = SubscribeAsync();
		}

		private async Task<IDisposable> SubscribeAsync()
		{
			DartAnalysisService analysisService = await analysisServiceFactory.GetAnalysisServiceAsync().ConfigureAwait(false);
			return analysisService.AnalysisNavigationNotification.Where(en => en.File == textDocument.FilePath).Subscribe(UpdateNavigationData);
		}

		void UpdateNavigationData(AnalysisNavigationNotification notification)
		{
			navigationNotification = notification;
		}

		protected override void Exec(uint nCmdID, IntPtr pvaIn)
		{
			var offset = textView.Caret.Position.BufferPosition.Position;
			var navigationRegion = navigationNotification.Regions.FirstOrDefault(r => r.Offset <= offset && r.Offset + r.Length >= offset);

			if (navigationRegion.Targets != null && navigationRegion.Targets.Any())
			{
				// TODO: Show user if there are multiple targets!
				var target = navigationRegion.Targets.First();
				var file = navigationNotification.Files[target];
				var position = navigationRegion.Offset;

				Helpers.OpenFileInPreviewTab(serviceProvider, file);
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
