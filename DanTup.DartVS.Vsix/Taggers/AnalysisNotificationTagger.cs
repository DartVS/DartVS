using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DanTup.DartVS
{
	abstract class AnalysisNotificationTagger<TTag, TSourceData, TNotificationType> : IDisposable, ITagger<TTag> where TTag : ITag
	{
		protected ITextBuffer buffer;
		protected ITextDocumentFactoryService textDocumentFactory;
		protected ITextDocument textDocument;
		protected DartAnalysisService analysisService;
		protected TSourceData[] currentData = new TSourceData[0];
		int earliestOffset = 0;
		int latestEnd = 0;

		IDisposable subscription;

		public AnalysisNotificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
		{
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.analysisService = analysisService;

			textDocumentFactory.TryGetTextDocument(this.buffer, out this.textDocument);

			// Subscribe to errors for the current file.
			subscription = this.Subscribe(UpdateSourceData);
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
			var newData = GetDataToTag(notification);

			var oldData = Interlocked.Exchange(ref currentData, newData);

			var handler = this.TagsChanged;
			if (handler != null && (oldData.Any() || newData.Any()))
			{
				// Get locations of all tags; existing and new, so that we can calculate the span that has changed.
				// TODO: Figure out if it's more efficient to do lots of small spans (each tag), or one big span...
				var allTags = oldData.Concat(newData);
				var allTagLocations = allTags.Select(GetOffsetAndLength);

				// Get the start/end of items, then calculate the length (EventArgs wants offset/length, not start/end offsets).
				var earliestOffset = allTagLocations.Min(l => l.Item1);
				var latestOffset = allTagLocations.Max(l => l.Item1 + l.Item2);
				var length = latestOffset - earliestOffset;

				// Clamp both values within the current buffer, in case we deleted a chunk, and there were old issues past the end
				// of the "current" document.
				earliestOffset = Math.Min(earliestOffset, buffer.CurrentSnapshot.Length);
				length = Math.Min(length, buffer.CurrentSnapshot.Length - earliestOffset);

				handler(this, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, earliestOffset, length)));
			}
		}

		public void Dispose()
		{
			subscription.Dispose();
		}

		protected abstract ITagSpan<TTag> CreateTag(TSourceData data);
		protected abstract IDisposable Subscribe(Action<TNotificationType> updateSourceData);
		protected abstract TSourceData[] GetDataToTag(TNotificationType notification);
		protected abstract Tuple<int, int> GetOffsetAndLength(TSourceData data);
	}
}
