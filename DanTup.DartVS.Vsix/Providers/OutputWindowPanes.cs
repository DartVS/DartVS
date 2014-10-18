using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Tvl.VisualStudio.OutputWindow.Interfaces;

namespace DanTup.DartVS.Providers
{
	internal static class OutputWindowPanes
	{
		public const string DartVSDiagnostics = "DartVS Diagnostics";

		[Export]
		[Name(DartVSDiagnostics)]
		private static readonly OutputWindowDefinition DartVSDiagnosticsPane;
	}
}
