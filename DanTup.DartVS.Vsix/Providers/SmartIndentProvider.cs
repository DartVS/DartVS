using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Providers
{
	[Export(typeof(ISmartIndentProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	class SmartIndentProvider : ISmartIndentProvider
	{
		public ISmartIndent CreateSmartIndent(ITextView textView)
		{
			var tabSize = textView.Options.Parent.IsOptionDefined("Tabs/TabSize", true) ? textView.Options.Parent.GetOptionValue<int>("Tabs/TabSize") : 2;
			return textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(tabSize));
		}
	}

	class SmartIndent : ISmartIndent
	{
		int tabSize;
		public SmartIndent(int tabSize)
		{
			this.tabSize = tabSize;
		}

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
			var previousLineIndent = previousLineText.Replace("\t", new string(' ', tabSize)).TakeWhile(char.IsWhiteSpace).Count();

			// If we started a block on the previous line; then add indent.
			if (previousLineText.TrimEnd().EndsWith("{"))
				return previousLineIndent + tabSize;
			else
				return previousLineIndent;
		}

		public void Dispose()
		{
		}
	}
}
