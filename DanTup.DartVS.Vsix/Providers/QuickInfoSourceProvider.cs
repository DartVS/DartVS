using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Providers
{
	[Export(typeof(IQuickInfoSourceProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[Name("Test")]
	[Order(Before = "Default Quick Info Presenter")]
	class QuickInfoSourceProvider : IQuickInfoSourceProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		DartAnalysisService analysisService = null;

		public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer)
		{
			return new QuickInfoSource(this, buffer, textDocumentFactory, analysisService);
		}
	}

	class QuickInfoSource : IQuickInfoSource
	{
		QuickInfoSourceProvider provider;
		ITextBuffer buffer;
		ITextDocumentFactoryService textDocumentFactory;
		DartAnalysisService analysisService;

		public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
		{
			this.provider = provider;
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.analysisService = analysisService;
		}

		public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
		{
			applicableToSpan = null;

			var triggerPoint = session.GetTriggerPoint(buffer.CurrentSnapshot);
			if (triggerPoint == null)
				return;

			ITextDocument doc;
			if (!textDocumentFactory.TryGetTextDocument(buffer, out doc))
				return;

			quickInfoContent.Add("Loading...");
			// TODO: Make this real; not just next 1 char
			applicableToSpan = buffer.CurrentSnapshot.CreateTrackingSpan(triggerPoint.Value.Position, 1, SpanTrackingMode.EdgeInclusive);

			var hoverTask = analysisService.GetHover(doc.FilePath, triggerPoint.Value.Position);
			hoverTask.ContinueWith(hovers =>
			{
				string tooltipData = null;
				if (hovers.Result.Length > 0 && hovers.Result[0] != null)
					tooltipData = hovers.Result[0].elementDescription ?? hovers.Result[0].parameter;

				if (tooltipData != null)
					session.QuickInfoContent[0] = tooltipData;
				else
					session.Dismiss();
			}, TaskScheduler.FromCurrentSynchronizationContext()); // TODO: Without this, Dismiss doesn't work; but is this a good way to do it?
		}

		public void Dispose()
		{
			GC.SuppressFinalize(true);
		}
	}
}
