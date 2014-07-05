using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	// Borrowed from WebEssentials...
	static class Helpers
	{
		public static void OpenFileInPreviewTab(string file)
		{
			IVsNewDocumentStateContext newDocumentStateContext = null;

			try
			{
				IVsUIShellOpenDocument3 openDoc3 = DartPackage.GetGlobalService<SVsUIShellOpenDocument>() as IVsUIShellOpenDocument3;

				Guid reason = VSConstants.NewDocumentStateReason.Navigation;
				newDocumentStateContext = openDoc3.SetNewDocumentState((uint)__VSNEWDOCUMENTSTATE.NDS_Provisional, ref reason);

				DartPackage.DTE.ItemOperations.OpenFile(file);
			}
			finally
			{
				if (newDocumentStateContext != null)
					newDocumentStateContext.Restore();
			}
		}

		public static IWpfTextView GetCurentTextView()
		{
			var componentModel = GetComponentModel();
			if (componentModel == null) return null;
			var editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();

			return editorAdapter.GetWpfTextView(GetCurrentNativeTextView());
		}

		public static IVsTextView GetCurrentNativeTextView()
		{
			var textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));

			IVsTextView activeView = null;
			ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out activeView));
			return activeView;
		}

		public static IComponentModel GetComponentModel()
		{
			return (IComponentModel)DartPackage.GetGlobalService(typeof(SComponentModel));
		}
	}
}
