using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartVS
{
	/// <summary>
	/// Provides access to Google's Dart Analysis service.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export]
	public class DartVsAnalysisService : DartAnalysis.DartAnalysisService
	{
		readonly DartProjectTracker projectTracker;
		readonly OpenFileTracker openFileTracker;
		readonly AnalysisService[] subscriptions = new[] { AnalysisService.Highlights, AnalysisService.Outline, AnalysisService.Navigation };

		public static readonly string ExtensionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static readonly string AnalysisServerScript = Path.Combine(ExtensionFolder, @"AnalysisServer.dart");

		[ImportingConstructor]
		public DartVsAnalysisService([Import]DartProjectTracker projectTracker, [Import]OpenFileTracker openFileTracker)
			: base(SdkPath, AnalysisServerScript)
		{
			this.projectTracker = projectTracker;
			this.openFileTracker = openFileTracker;

			// TODO: Only fire this up when there's at least one Dart project open!
			// TODO: Shut it down when the last dart project closes!

			// When Dart projects change; update analysis roots.
			this.projectTracker.ProjectsChanged.Subscribe(projs => this.SetAnalysisRoots(projs.Select(p => p.Path).ToArray()));

			// When open files change; update subscriptions.
			this.openFileTracker.DocumentsChanged.Subscribe(files => this.SetAnalysisSubscriptions(subscriptions.ToDictionary(s => s, s => files)));
		}

		public static string SdkPath
		{
			get
			{
				string result = Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Process);
				if (!Directory.Exists(result))
				{
					// TODO: These should be updated to reference shared constants
					string extensionName = "DartVS";
					string extensionVersion = "0.5";
					string tempDir = Path.Combine(Path.GetTempPath(), string.Format("{0}-{1}-sdk", extensionName, extensionVersion));
					result = Path.Combine(tempDir, "dart-sdk");
					if (!Directory.Exists(result))
					{
						Directory.CreateDirectory(tempDir);
						string compressed = Path.Combine(Path.GetDirectoryName(typeof(DartVsAnalysisService).Assembly.Location), "SDK", "dartsdk-windows-ia32-release.zip");
						ZipFile.ExtractToDirectory(compressed, tempDir);
					}
				}

				return result;
			}
		}
	}
}
