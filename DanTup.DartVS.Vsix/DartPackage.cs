using System;
using System.ComponentModel.Composition;
using System.IO;
using DanTup.DartAnalysis;
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

		VsDocumentEvents events;
		DartErrorListProvider errorProvider;
		ErrorListProvider errorListProvider;

		protected override void Initialize()
		{
			base.Initialize();

			var componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
			componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

			errorProvider = new DartErrorListProvider(this);
			analysisService.AnalysisErrorsNotification.Subscribe(errorProvider.UpdateErrors);

			IconRegistration.RegisterIcons();
		}
		
		protected override void Dispose(bool disposing)
		{
			events.Dispose();
		}
	}
}
