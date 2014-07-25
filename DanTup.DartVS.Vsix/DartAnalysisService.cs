using System;
using System.ComponentModel.Composition;
using System.Linq;
using DanTup.DartAnalysis;

namespace DanTup.DartVS
{
	/// <summary>
	/// Provides access to Google's Dart Analysis service.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export]
	public class DartAnalysisService : DartAnalysis.DartAnalysisService
	{
		readonly DartProjectTracker projectTracker;
		readonly OpenFileTracker openFileTracker;
		readonly AnalysisSubscription[] subscriptions = new[] { AnalysisSubscription.Errors, AnalysisSubscription.Highlights, AnalysisSubscription.Outline, AnalysisSubscription.Navigation };

		static readonly string SdkPath =
			Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.User)
			?? Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Machine);

		[ImportingConstructor]
		public DartAnalysisService([Import]DartProjectTracker projectTracker, [Import]OpenFileTracker openFileTracker)
			: base(SdkPath, @"M:\Apps\Dart\AnalysisServer\\analysis_server.dart.snapshot")
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
	}
}
