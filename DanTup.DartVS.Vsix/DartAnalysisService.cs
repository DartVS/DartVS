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
		DartProjectTracker projectTracker;

		static readonly string SdkPath =
			Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.User)
			?? Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Machine);

		[ImportingConstructor]
		public DartAnalysisService([Import]DartProjectTracker projectTracker)
			: base(SdkPath, @"M:\Coding\TestApps\DartServerTest\bin\server.dart")
		{
			this.projectTracker = projectTracker;

			// When Dart projects change; update analysis roots
			this.projectTracker.ProjectsChanged.Subscribe(projs => this.SetAnalysisRoots(projs.Select(p => p.Path).ToArray()));
		}
	}
}
