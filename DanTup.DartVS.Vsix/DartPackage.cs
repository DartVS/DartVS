using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("DanTup's Dart support for Visual Studio", @"Some support for coding Dart in Visual Studio.", "0.1")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	public sealed class DartPackage : Package
	{
		VsDocumentEvents events;
		ErrorListProvider errorListProvider;

		protected override void Initialize()
		{
			base.Initialize();

			events = new VsDocumentEvents();
			errorListProvider = new ErrorListProvider(this);

			events.FileSaved += events_FileSaved;
		}

		void events_FileSaved(object sender, string filename)
		{
			// Kick off a thread; because it might be slow!
			var isDartFile = string.Equals(Path.GetExtension(filename), ".dart", StringComparison.OrdinalIgnoreCase);
			if (isDartFile)
				System.Threading.Tasks.Task.Run(() => DartAnalyzerErrorService.Parse(errorListProvider, filename));
		}

		protected override void Dispose(bool disposing)
		{
			events.Dispose();
		}
	}
}
