using System;
using System.ComponentModel.Composition;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;
using Stream = System.IO.Stream;

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

		private static async Task<string> GetSdkPathAsync(CancellationToken cancellationToken)
		{
			string result = Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Process);
			if (!Directory.Exists(result))
			{
				string extensionName = "DartVS";
				string extensionVersion = AssemblyInfo.AssemblyInformationalVersion;
				string tempDir = Path.Combine(Path.GetTempPath(), string.Format("{0}-{1}-sdk", extensionName, extensionVersion));
				result = Path.Combine(tempDir, "dart-sdk");
				if (!Directory.Exists(result))
				{
					Directory.CreateDirectory(tempDir);
					string sdkName = "dartsdk-windows-ia32-release.zip";
					string compressed = Path.Combine(tempDir, sdkName);

					using (HttpClient httpClient = new HttpClient())
					{
						using (Stream stream = await httpClient.GetStreamAsync("https://storage.googleapis.com/dart-archive/channels/stable/release/latest/sdk/dartsdk-windows-ia32-release.zip").ConfigureAwait(false))
						{
							using (var outputStream = File.OpenWrite(compressed))
							{
								await stream.CopyToAsync(outputStream).ConfigureAwait(false);
							}
						}
					}

					ZipFile.ExtractToDirectory(compressed, tempDir);
				}
			}

			if (!Directory.Exists(result) || !File.Exists(Path.Combine(result, "bin", "dart.exe")))
				throw new NotSupportedException("Could not locate or download the Dart SDK. All analysis is disabled.");

			return result;
		}

		public Task<DartAnalysisService> GetAnalysisServiceAsync()
		{
			return getAnalysisServerTask.Value;
		}

		private async Task<DartAnalysisService> StartGetAnalysisServiceAsync()
		{
			string sdkPath = await GetSdkPathAsync(CancellationToken.None).ConfigureAwait(false);
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
