using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartConstants.ContentType)]
	[TagType(typeof(ErrorTag))]
	internal sealed class ErrorSquiggleTagProvider : ITaggerProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		DartAnalysisServiceFactory analysisServiceFactory = null;

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			return new ErrorSquiggleTagger(buffer, textDocumentFactory, analysisServiceFactory) as ITagger<T>;
		}
	}

	class ErrorSquiggleTagger : AnalysisNotificationTagger<ErrorTag, AnalysisError, AnalysisErrorsNotification>
	{
		public ErrorSquiggleTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisServiceFactory analysisServiceFactory)
			: base(buffer, textDocumentFactory, analysisServiceFactory)
		{
			this.Subscribe();
		}

		protected override ITagSpan<ErrorTag> CreateTag(AnalysisError error)
		{
			// syntax error: red
			// compiler error: blue
			// other error: purple
			// warning: red
			var squiggleType = error.Severity == AnalysisErrorSeverity.Error ? PredefinedErrorTypeNames.SyntaxError
				: error.Severity == AnalysisErrorSeverity.Warning ? PredefinedErrorTypeNames.CompilerError
				: PredefinedErrorTypeNames.OtherError;

			return new TagSpan<ErrorTag>(new SnapshotSpan(buffer.CurrentSnapshot, error.Location.Offset, error.Location.Length), new ErrorTag(squiggleType, error.Message));
		}

		protected override async Task<IDisposable> SubscribeAsync(Action<AnalysisErrorsNotification> updateSourceData)
		{
			DartAnalysisService analysisService = await analysisServiceFactory.GetAnalysisServiceAsync().ConfigureAwait(false);
			return analysisService.AnalysisErrorsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(updateSourceData);
		}

		protected override AnalysisError[] GetDataToTag(AnalysisErrorsNotification notification)
		{
			return notification.Errors.Where(e => e.Location.File == textDocument.FilePath).ToArray();
		}

		protected override Tuple<int, int> GetOffsetAndLength(AnalysisError data)
		{
			return Tuple.Create(data.Location.Offset, data.Location.Length);
		}
	}
}
