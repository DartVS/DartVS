using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("DanTup's DartVS: Visual Studio support for Google's Dart", @"Some support for coding Dart in Visual Studio.", AssemblyInfo.InstalledProductVersion)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	[ProvideLanguageService(typeof(DartLanguageInfo), DartConstants.LanguageName, 100)]
	[ProvideLanguageExtension(typeof(DartLanguageInfo), DartConstants.FileExtension)]

	[ProvideOptionPage(typeof(OptionsPages.OptionsPageGeneral), "DartVS", "General", 1000, 1001, true)]

	[ProvideBindingPath]
	[Guid(DartConstants.PackageGuidString)]
	public sealed class DartPackage : Package
	{
		[Import]
		DartAnalysisServiceFactory analysisServiceFactory = null;

		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		IVsEditorAdaptersFactoryService editorAdapterFactory = null;

		DartErrorListProvider errorProvider;
		Task<IDisposable> errorsSubscription;

		// TODO: Handle file renames properly (errors stick around)
		// TODO: Handle closing projects/solutions (errors stick around)

		public OptionsPages.OptionsPageGeneral OptionsPageGeneral
		{
			get
			{
				return GetDialogPage<OptionsPages.OptionsPageGeneral>();
			}
		}

		protected override void Initialize()
		{
			base.Initialize();

			// Force initialisation of [Imports] on this class.
			var componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
			componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

			// Wire up the Error Provider to the notifications from the service.
			errorProvider = new DartErrorListProvider(this);
			errorsSubscription = SubscribeAsync(errorProvider);

			// Register icons so they show in the solution explorer nicely.
			IconRegistration.RegisterIcons();

			((IServiceContainer)this).AddService(typeof(DartLanguageInfo), new DartLanguageInfo(textDocumentFactory, editorAdapterFactory, analysisServiceFactory), true);
		}

		private async Task<IDisposable> SubscribeAsync(DartErrorListProvider errorProvider)
		{
			DartAnalysisService analysisService = await analysisServiceFactory.GetAnalysisServiceAsync().ConfigureAwait(false);
			return analysisService.AnalysisErrorsNotification.Subscribe(errorProvider.UpdateErrors);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Task<IDisposable> errorsSubscriptionTask = errorsSubscription;
				if (errorsSubscriptionTask != null)
					errorsSubscriptionTask.ContinueWith(task => task.Result.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion);
			}
		}

		public static T GetGlobalService<T>(Type type = null) where T : class
		{
			return Package.GetGlobalService(type ?? typeof(T)) as T;
		}

		T GetDialogPage<T>()
			where T : DialogPage
		{
			return (T)GetDialogPage(typeof(T));
		}
	}
}
