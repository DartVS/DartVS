using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[TagType(typeof(ErrorTag))]
	internal sealed class ErrorSquiggleTagProvider : ITaggerProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		DartAnalysisService analysisService = null;

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			return new ErrorSquiggleTagger(buffer, textDocumentFactory, analysisService) as ITagger<T>;
		}
	}

	class ErrorSquiggleTagger : AnalysisNotificationTagger<ErrorTag, AnalysisError, AnalysisErrorsEvent>
	{
		public ErrorSquiggleTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
			: base(buffer, textDocumentFactory, analysisService)
		{
			this.Subscribe();
		}

		protected override ITagSpan<ErrorTag> CreateTag(AnalysisError error)
		{
			// syntax error: red
			// compiler error: blue
			// other error: purple
			// warning: red
			var squiggleType = error.Severity == AnalysisErrorSeverity.Error ? "syntax error"
				: error.Severity == AnalysisErrorSeverity.Warning ? "compiler error"
				: "other error";

			return new TagSpan<ErrorTag>(new SnapshotSpan(buffer.CurrentSnapshot, error.Location.Offset, error.Location.Length), new ErrorTag(squiggleType, error.Message));
		}

		protected override IDisposable Subscribe(Action<AnalysisErrorsEvent> updateSourceData)
		{
			return this.analysisService.AnalysisErrorsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(updateSourceData);
		}

		protected override AnalysisError[] GetDataToTag(AnalysisErrorsEvent notification)
		{
			return notification.Errors.Where(e => e.Location.File == textDocument.FilePath).ToArray();
		}

		protected override Tuple<int, int> GetOffsetAndLength(AnalysisError data)
		{
			return Tuple.Create(data.Location.Offset, data.Location.Length);
		}
	}
}
