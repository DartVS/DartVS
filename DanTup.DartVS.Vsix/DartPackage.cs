using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("DanTup's DartVS: Visual Studio support for Google's Dart", @"Some support for coding Dart in Visual Studio.", "0.5")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	public sealed class DartPackage : Package
	{
		[Import]
		public DartAnalysisService analysisService = null;

		DartErrorListProvider errorProvider;
		DartFileChangeTracker changeTracking;

		protected override void Initialize()
		{
			base.Initialize();

			var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

			// Force initialisation of [Imports] on this class.
			var componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
			componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

			// Wire up the Error Provider to the notifications from the service.
			errorProvider = new DartErrorListProvider(dte, this);
			analysisService.AnalysisErrorsNotification.Subscribe(errorProvider.UpdateErrors);

			// Wire up document change tracking to the service.
			changeTracking = new DartFileChangeTracker(dte, analysisService);

			// Register icons so they show in the solution explorer nicely.
			IconRegistration.RegisterIcons();
		}

		protected override void Dispose(bool disposing)
		{
			analysisService.Dispose();
		}
	}
}
