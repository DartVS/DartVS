using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Providers
{
	// TODO: Currently this breaks when VS converts 4 spaces to 1 tab. To fix, we really need to
	// know if VS is going to covert x spaces to tabs (and then count a tab as that many characters)
	// However; we really also need to be able to register Dart as a language and set it to default to
	// 2 spaces (no tabs); so we can fix this all up toegther if I ever figure out how to do it.

	// TODO: Remove this export when it's ready to use.
	//[Export(typeof(ISmartIndentProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	class SmartIndentProvider : ISmartIndentProvider
	{
		public ISmartIndent CreateSmartIndent(ITextView textView)
		{
			return textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent());
		}
	}

	class SmartIndent : ISmartIndent
	{
		public int? GetDesiredIndentation(ITextSnapshotLine line)
		{
			// If we're on the first line, we can't really do anything clever.
			if (line.LineNumber == 0)
				return 0;

			var snapshot = line.Snapshot;

			// Walk up previous lines trying to find the first non-blank one.
			var previousNonBlankLine = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
			while (previousNonBlankLine.LineNumber >= 1 && previousNonBlankLine.GetText().Trim().Length == 0)
				previousNonBlankLine = snapshot.GetLineFromLineNumber(previousNonBlankLine.LineNumber - 1);

			// If we didn't find an actual non-blank line, we can't really do anything clever.
			if (previousNonBlankLine.GetText().Trim() == "")
				return 0;

			var previousLineText = previousNonBlankLine.GetText();
			var thisLineText = line.GetText();
			var previousLineIndent = previousLineText.TakeWhile(char.IsWhiteSpace).Count();

			if (previousLineText.TrimEnd().EndsWith("{"))
				return previousLineIndent + 2;
			else if (thisLineText.TrimStart().StartsWith("}") && previousLineIndent >= 2)
				return previousLineIndent - 2;
			else
				return previousLineIndent;
		}

		public void Dispose()
		{
		}
	}
}
