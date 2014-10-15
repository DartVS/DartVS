using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartConstants.ContentType)]
	[TagType(typeof(ClassificationTag))]
	internal sealed class ClassificationTagProvider : ITaggerProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		DartAnalysisServiceFactory analysisServiceFactory = null;

		[Import]
		internal IClassificationTypeRegistryService classificationTypeRegistry = null;

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			return new ClassificationTagger(buffer, textDocumentFactory, analysisServiceFactory, classificationTypeRegistry) as ITagger<T>;
		}
	}

	class ClassificationTagger : AnalysisNotificationTagger<ClassificationTag, HighlightRegion, AnalysisHighlightsNotification>
	{
		IClassificationType[] classificationMapping;

		public ClassificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisServiceFactory analysisServiceFactory, IClassificationTypeRegistryService typeService)
			: base(buffer, textDocumentFactory, analysisServiceFactory)
		{
			Array values = Enum.GetValues(typeof(HighlightRegionType));
			classificationMapping = new IClassificationType[values.Length];
			for (int i = 0; i < classificationMapping.Length; i++)
				classificationMapping[i] = typeService.GetClassificationType(DartConstants.ContentType + ((HighlightRegionType)i).ToString());

			this.Subscribe();
		}

		protected override ITagSpan<ClassificationTag> CreateTag(HighlightRegion highlight)
		{
			return new TagSpan<ClassificationTag>(new SnapshotSpan(buffer.CurrentSnapshot, highlight.Offset, highlight.Length), new ClassificationTag(classificationMapping[(int)highlight.Type]));
		}

		protected override async Task<IDisposable> SubscribeAsync(Action<AnalysisHighlightsNotification> updateSourceData)
		{
			DartAnalysisService analysisService = await analysisServiceFactory.GetAnalysisServiceAsync().ConfigureAwait(false);
			return analysisService.AnalysisHighlightsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(updateSourceData);
		}

		protected override HighlightRegion[] GetDataToTag(AnalysisHighlightsNotification notification)
		{
			// HACK: This happens if when the base constructor Subscribes, we already ahve an event in the stream...
			// TODO: Make this better. Ideally, we need to set classificationMapping before the base class Subscribes...
			if (classificationMapping == null)
				return new HighlightRegion[0];

			return notification.Regions.Where(h => h.Type >= 0 && (int)h.Type < classificationMapping.Length).ToArray();
		}

		protected override Tuple<int, int> GetOffsetAndLength(HighlightRegion data)
		{
			return Tuple.Create(data.Offset, data.Length);
		}
	}
}
