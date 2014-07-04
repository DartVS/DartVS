using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("DanTup's DartVS: Visual Studio support for Google's Dart", @"Some support for coding Dart in Visual Studio.", "0.5")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	[ProvideLanguageService(typeof(DartLanguageInfo), "Dart", 100)]
	[ProvideLanguageExtension(typeof(DartLanguageInfo), ".dart")]
	public sealed class DartPackage : Package
	{
		[Import]
		public DartAnalysisService analysisService = null;

		DartErrorListProvider errorProvider;
		DartFileChangeTracker changeTracking;

		// TODO: Handle file renames properly (errors stick around)
		// TODO: Handle closing projects/solutions (errors stick around)

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

			((IServiceContainer)this).AddService(typeof(DartLanguageInfo), new DartLanguageInfo(), true);
		}

		protected override void Dispose(bool disposing)
		{
			analysisService.Dispose();
		}
	}
}
