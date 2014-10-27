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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Tvl.VisualStudio.OutputWindow.Interfaces;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("#101", "#102", AssemblyInfo.InstalledProductVersion)]
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

		/// <summary>
		/// This error list provider is used to warn developers if the VSBase Services Debugging Support extension is
		/// not installed.
		/// </summary>
		ErrorListProvider vsbaseWarningProvider;

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

			// Warn users if dependencies aren't installed.
			vsbaseWarningProvider = new ErrorListProvider(this);
			if (componentModel.DefaultExportProvider.GetExportedValueOrDefault<IOutputWindowService>() == null)
			{
				ErrorTask task = new ErrorTask()
				{
					Category = TaskCategory.Misc,
					ErrorCategory = TaskErrorCategory.Error,
					Text = "The required VSBase Services debugging support extension is not installed. Click here for more information."
				};
				task.Navigate += HandleNavigateToVsBaseServicesExtension;
				vsbaseWarningProvider.Tasks.Add(task);
				vsbaseWarningProvider.Show();
			}

			// Register icons so they show in the solution explorer nicely.
			IconRegistration.RegisterIcons();

			((IServiceContainer)this).AddService(typeof(DartLanguageInfo), new DartLanguageInfo(textDocumentFactory, editorAdapterFactory, analysisServiceFactory), true);
		}

		private void HandleNavigateToVsBaseServicesExtension(object sender, EventArgs e)
		{
			IVsWebBrowsingService webBrowsingService = GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
			if (webBrowsingService != null)
			{
				IVsWindowFrame windowFrame;
				webBrowsingService.Navigate("https://visualstudiogallery.msdn.microsoft.com/fca95a59-3fc6-444e-b20c-cc67828774cd", 0, out windowFrame);
				return;
			}

			IVsUIShellOpenDocument openDocument = GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
			if (openDocument != null)
			{
				openDocument.OpenStandardPreviewer(0, "https://visualstudiogallery.msdn.microsoft.com/fca95a59-3fc6-444e-b20c-cc67828774cd", VSPREVIEWRESOLUTION.PR_Default, 0);
				return;
			}
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

				if (vsbaseWarningProvider != null)
					vsbaseWarningProvider.Dispose();
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
