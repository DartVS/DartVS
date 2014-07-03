using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DanTup.DartVS.Taggers
{
	abstract class AnalysisNotificationTagger<TTag, TSourceData, TNotificationType> : ITagger<TTag> where TTag : ITag
	{
		protected ITextBuffer buffer;
		protected ITextDocumentFactoryService textDocumentFactory;
		protected ITextDocument textDocument;
		protected DartAnalysisService analysisService;
		protected TSourceData[] currentData = new TSourceData[0];
		int earliestOffset = 0;
		int latestEnd = 0;

		public AnalysisNotificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
		{
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.analysisService = analysisService;

			textDocumentFactory.TryGetTextDocument(this.buffer, out this.textDocument);

			// Subscribe to errors for the current file.
			this.Subscribe(UpdateSourceData);
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			foreach (var data in currentData)
			{
				var location = GetOffsetAndLength(data);

				// VS will crash if we pass an invalid span. Since our error might arrive after more changes are made; we'll have to filter
				// them out here.
				if (location.Item1 + location.Item2 > buffer.CurrentSnapshot.Length)
					continue;

				yield return CreateTag(data);
			}
		}

		private void UpdateSourceData(TNotificationType notification)
		{
			var dataToTag = GetDataToTag(notification);

			Interlocked.Exchange(ref currentData, dataToTag);

			if (dataToTag.Any())
			{
				// Figure out the smallest span that we have errors for.
				var locationInfo = dataToTag.Select(e => GetOffsetAndLength(e));
				var newEarliestOffset = locationInfo.Min(e => e.Item1);
				var newLatestEnd = locationInfo.Max(e => e.Item1 + e.Item2);

				var startPositionToUse = Math.Min(earliestOffset, newEarliestOffset);
				var endPositionToUse = Math.Max(latestEnd, newLatestEnd);
				var lengthToUse = Math.Min(endPositionToUse - startPositionToUse, buffer.CurrentSnapshot.Length - startPositionToUse);

				var handler = this.TagsChanged;
				if (handler != null)
					// Use the biggest span that includes boh previous and new errors, so we correctly remove any that are gone.
					handler(this, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, startPositionToUse, lengthToUse)));

				// Remember for next time round.
				earliestOffset = newEarliestOffset;
				latestEnd = newLatestEnd;
			}
			else
			{
				// No new errors, so just invalidate the old ones.
				var startPositionToUse = earliestOffset;
				var endPositionToUse = Math.Min(latestEnd, buffer.CurrentSnapshot.Length);
				var lengthToUse = Math.Min(endPositionToUse - startPositionToUse, buffer.CurrentSnapshot.Length - startPositionToUse);

				var handler = this.TagsChanged;
				if (handler != null)
					// Use the biggest span that includes boh previous and new errors, so we correctly remove any that are gone.
					handler(this, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, earliestOffset, lengthToUse)));

				earliestOffset = 0;
				latestEnd = 0;
			}
		}

		protected abstract ITagSpan<TTag> CreateTag(TSourceData data);
		protected abstract void Subscribe(Action<TNotificationType> updateSourceData);
		protected abstract TSourceData[] GetDataToTag(TNotificationType notification);
		protected abstract Tuple<int, int> GetOffsetAndLength(TSourceData data);
	}
}
