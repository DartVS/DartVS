using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	[InstalledProductRegistration("DanTup's Dart support for Visual Studio", @"Some support for coding Dart in Visual Studio.", "0.1")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	public sealed class DartPackage : Package
	{
	}
}
