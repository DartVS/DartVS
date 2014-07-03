using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Taggers
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

	class ErrorSquiggleTagger : ITagger<ErrorTag>
	{
		ITextBuffer buffer;
		ITextDocumentFactoryService textDocumentFactory;
		ITextDocument textDocument;
		DartAnalysisService analysisService;
		AnalysisError[] currentErrors = new AnalysisError[0];
		int earliestErrorOffset = 0;
		int latestErrorEnd = 0;

		public ErrorSquiggleTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
		{
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.analysisService = analysisService;

			textDocumentFactory.TryGetTextDocument(this.buffer, out this.textDocument);

			// Subscribe to errors for the current file.
			this.analysisService.AnalysisErrorsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(UpdateErrors);
		}

		public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			foreach (var error in currentErrors)
			{
				// VS will crash if we pass an invalid span. Since our error might arrive after more changes are made; we'll have to filter
				// them out here.
				if (error.Location.Offset + error.Location.Length > buffer.CurrentSnapshot.Length)
					continue;

				yield return new TagSpan<ErrorTag>(new SnapshotSpan(buffer.CurrentSnapshot, error.Location.Offset, error.Location.Length), new ErrorTag("syntax error", error.Message));
			}
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		private void UpdateErrors(AnalysisErrorsNotification notification)
		{
			var errorsForThisFile = notification.Errors.Where(e => e.Location.File == textDocument.FilePath).ToArray();

			Interlocked.Exchange(ref currentErrors, errorsForThisFile);

			if (errorsForThisFile.Any())
			{
				// Figure out the smallest span that we have errors for.
				var newEarliestErrorOffset = errorsForThisFile.Min(e => e.Location.Offset);
				var newLatestErrorEnd = errorsForThisFile.Max(e => e.Location.Offset + e.Location.Length);

				var startPositionToUse = Math.Min(earliestErrorOffset, newEarliestErrorOffset);
				var endPositionToUse = Math.Max(latestErrorEnd, newLatestErrorEnd);
				var lengthToUse = Math.Min(endPositionToUse - startPositionToUse, buffer.CurrentSnapshot.Length - startPositionToUse);

				var handler = this.TagsChanged;
				if (handler != null)
					// Use the biggest span that includes boh previous and new errors, so we correctly remove any that are gone.
					handler(this, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, startPositionToUse, lengthToUse)));

				// Remember for next time round.
				earliestErrorOffset = newEarliestErrorOffset;
				latestErrorEnd = newLatestErrorEnd;
			}
			else
			{
				// No new errors, so just invalidate the old ones.

				var startPositionToUse = earliestErrorOffset;
				var endPositionToUse = Math.Min(latestErrorEnd, buffer.CurrentSnapshot.Length);
				var lengthToUse = Math.Min(endPositionToUse - startPositionToUse, buffer.CurrentSnapshot.Length - startPositionToUse);


				var handler = this.TagsChanged;
				if (handler != null)
					// Use the biggest span that includes boh previous and new errors, so we correctly remove any that are gone.
					handler(this, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, earliestErrorOffset, lengthToUse)));

				earliestErrorOffset = 0;
				latestErrorEnd = 0;
			}
		}
	}
}
