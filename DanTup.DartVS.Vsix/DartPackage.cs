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

		protected override void Initialize()
		{
			base.Initialize();

			var componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
			componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

			var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
			errorProvider = new DartErrorListProvider(dte, this);
			analysisService.AnalysisErrorsNotification.Subscribe(errorProvider.UpdateErrors);

			IconRegistration.RegisterIcons();
		}

		protected override void Dispose(bool disposing)
		{
			analysisService.Dispose();
		}
	}
}
