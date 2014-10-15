using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(IQuickInfoSourceProvider))]
	[ContentType(DartConstants.ContentType)]
	[Name("Dart Quick")]
	class QuickInfoSourceProvider : IQuickInfoSourceProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		IBufferTagAggregatorFactoryService tagAggregatorService = null;

		[Import]
		DartAnalysisServiceFactory analysisServiceFactory = null;

		public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer)
		{
			return new QuickInfoSource(this, buffer, textDocumentFactory, tagAggregatorService.CreateTagAggregator<ClassificationTag>(buffer), analysisServiceFactory);
		}
	}

	class QuickInfoSource : IQuickInfoSource
	{
		QuickInfoSourceProvider provider;
		ITextBuffer buffer;
		ITextDocumentFactoryService textDocumentFactory;
		ITagAggregator<ClassificationTag> tagAggregator;
		DartAnalysisServiceFactory analysisServiceFactory;

		public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, ITagAggregator<ClassificationTag> tagAggregator, DartAnalysisServiceFactory analysisServiceFactory)
		{
			this.provider = provider;
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.tagAggregator = tagAggregator;
			this.analysisServiceFactory = analysisServiceFactory;
		}

		int? inProgressPosition = null;
		string inProgressTooltipData = null;
		ITrackingSpan inProgressApplicableToSpan = null;


		public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
		{
			applicableToSpan = null;

			var triggerPoint = session.GetTriggerPoint(buffer.CurrentSnapshot);
			if (triggerPoint == null)
				return;

			ITextDocument doc;
			if (!textDocumentFactory.TryGetTextDocument(buffer, out doc))
				return;

			// Figure out if this is a recalculate for an existing span (not sure if this is the best way of supporting async...?)
			if (inProgressPosition != null && inProgressPosition.Value == triggerPoint.Value.Position)
			{
				UpdateTooltip(session, quickInfoContent, out applicableToSpan);
			}
			else
			{
				applicableToSpan = GetApplicableToSpan(triggerPoint);
				var ignoredTask = StartTooltipRequestAsync(session, quickInfoContent, applicableToSpan, triggerPoint, doc.FilePath);
			}
		}

		async Task StartTooltipRequestAsync(IQuickInfoSession session, IList<object> quickInfoContent, ITrackingSpan applicableToSpan, SnapshotPoint? triggerPoint, string filePath)
		{
			// If this position didn't have a classification, then it's uninteresting, and won't have tooltips.
			if (applicableToSpan == null)
				return;

			// Set the position so we know what request is in process.
			inProgressPosition = triggerPoint.Value.Position;
			inProgressTooltipData = null;
			inProgressApplicableToSpan = null;

			// Put dummy content in tooltip while the request in in-flight.
			quickInfoContent.Add("Loading...");

			// Fire off a request to the service to get the data.
			DartAnalysisService analysisService = await analysisServiceFactory.GetAnalysisServiceAsync().ConfigureAwait(false);
			HoverInformation[] hovers = await analysisService.GetHover(filePath, triggerPoint.Value.Position);

			// Build the tooltip info if the response was valid.
			var tooltipData = BuildTooltip(hovers);

			if (!string.IsNullOrWhiteSpace(tooltipData))
			{
				// Stash the data for the next call, and tell VS to reclaculate now that we have the good info.
				inProgressTooltipData = tooltipData;
				inProgressApplicableToSpan = buffer.CurrentSnapshot.CreateTrackingSpan(hovers[0].Offset, hovers[0].Length, SpanTrackingMode.EdgeInclusive);
				session.Recalculate();
			}
			else
			{
				// Otherwise, no valid response, means no tooltip.
				session.Dismiss();
			}
		}

		ITrackingSpan GetApplicableToSpan(SnapshotPoint? triggerPoint)
		{
			// Attempt to use the Classicification data to get the span
			var classificationTags = tagAggregator.GetTags(new SnapshotSpan(triggerPoint.Value, 0));
			var classificationTag = classificationTags.FirstOrDefault();

			if (classificationTag != null)
				return buffer.CurrentSnapshot.CreateTrackingSpan(classificationTag.Span.GetSpans(this.buffer).First().Span, SpanTrackingMode.EdgeInclusive);
			else
				return null; // If something wasn't in the Classifications, then it's not interesting.
		}

		void UpdateTooltip(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
		{
			// Provide the tooltip data from the data we stashed in our callback.
			quickInfoContent.Add(inProgressTooltipData);
			// TODO: For some reason, this doesn't seem to work properly; the tooltip flickers as the mouse moves (and fires off additional requests) :(
			applicableToSpan = inProgressApplicableToSpan;
		}

		string BuildTooltip(HoverInformation[] hovers)
		{
			if (hovers == null || hovers.Length == 0 || hovers[0] == null)
				return null;

			var typeInfo = hovers[0].ElementDescription ?? hovers[0].Parameter;

			return string.Format("{0}\r\n{1}", typeInfo, hovers[0].Dartdoc).Trim();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(true);
		}
	}
}
