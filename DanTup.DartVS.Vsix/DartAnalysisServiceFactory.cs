using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using DartVS;
using Path = System.IO.Path;

namespace DanTup.DartVS
{
	[Export]
	internal class DartAnalysisServiceFactory
	{
		readonly DartProjectTracker projectTracker;
		readonly OpenFileTracker openFileTracker;
		readonly Lazy<Task<DartAnalysisService>> getAnalysisServerTask;

		public static readonly string ExtensionFolder = Path.GetDirectoryName(typeof(DartAnalysisServiceFactory).Assembly.Location);
		public static readonly string AnalysisServerScript = Path.Combine(ExtensionFolder, @"AnalysisServer.dart");

		[ImportingConstructor]
		public DartAnalysisServiceFactory(DartProjectTracker projectTracker, OpenFileTracker openFileTracker)
		{
			this.projectTracker = projectTracker;
			this.openFileTracker = openFileTracker;
			this.getAnalysisServerTask = new Lazy<Task<DartAnalysisService>>(StartGetAnalysisServiceAsync, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public Task<DartAnalysisService> GetAnalysisServiceAsync()
		{
			return getAnalysisServerTask.Value;
		}

		private async Task<DartAnalysisService> StartGetAnalysisServiceAsync()
		{
			string sdkPath = await DartSdk.GetSdkPathAsync().ConfigureAwait(false);
			return new DartVsAnalysisService(sdkPath, AnalysisServerScript, projectTracker, openFileTracker);
		}

		private class DartVsAnalysisService : DartAnalysisService
		{
			readonly DartProjectTracker projectTracker;
			readonly OpenFileTracker openFileTracker;
			readonly AnalysisService[] subscriptions = new[] { AnalysisService.Highlights, AnalysisService.Outline, AnalysisService.Navigation };

			readonly CompositeDisposable subscriptionDisposable = new CompositeDisposable();

			public DartVsAnalysisService(string sdkPath, string analysisServerScript, DartProjectTracker projectTracker, OpenFileTracker openFileTracker)
				: base(sdkPath, analysisServerScript)
			{
				this.projectTracker = projectTracker;
				this.openFileTracker = openFileTracker;

				// TODO: Only fire this up when there's at least one Dart project open!
				// TODO: Shut it down when the last dart project closes!

				// When Dart projects change; update analysis roots.
				subscriptionDisposable.Add(this.projectTracker.ProjectsChanged.Subscribe(projs => this.SetAnalysisRoots(projs.Select(p => p.Path).ToArray())));

				// When open files change; update subscriptions.
				subscriptionDisposable.Add(this.openFileTracker.DocumentsChanged.Subscribe(files => this.SetAnalysisSubscriptions(subscriptions.ToDictionary(s => s, s => files))));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					subscriptionDisposable.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
