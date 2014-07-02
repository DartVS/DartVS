using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(IBraceCompletionDefaultProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[BracePair('{', '}')]
	[BracePair('(', ')')]
	[BracePair('[', ']')]
	class BraceCompletionProvider : IBraceCompletionDefaultProvider
	{
	}
}
