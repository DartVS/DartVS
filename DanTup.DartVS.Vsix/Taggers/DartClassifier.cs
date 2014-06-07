using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Taggers
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[TagType(typeof(ClassificationTag))]
	internal sealed class DartClassifierProvider : ITaggerProvider
	{
		[Import]
		internal IClassificationTypeRegistryService classificationTypeRegistry = null;

		[Import]
		internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			var dartTagAggregator = aggregatorFactory.CreateTagAggregator<DartTokenTag>(buffer);

			return new DartClassifier(buffer, dartTagAggregator, classificationTypeRegistry) as ITagger<T>;
		}
	}

	internal sealed class DartClassifier : ITagger<ClassificationTag>
	{
		ITextBuffer buffer;
		ITagAggregator<DartTokenTag> aggregator;
		IDictionary<DartTokenType, IClassificationType> classificationMapping;

		internal DartClassifier(ITextBuffer buffer, ITagAggregator<DartTokenTag> aggregator, IClassificationTypeRegistryService typeService)
		{
			this.buffer = buffer;
			this.aggregator = aggregator;

			// Mapping of DartTokenTypes to the VS Classifications we wish to give them
			classificationMapping = new Dictionary<DartTokenType, IClassificationType> {
				{ DartTokenType.Comment, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ DartTokenType.Identifier, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition) },
				{ DartTokenType.Keyword, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ DartTokenType.Number, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				{ DartTokenType.Operator, typeService.GetClassificationType(PredefinedClassificationTypeNames.Operator) },
				{ DartTokenType.String, typeService.GetClassificationType(PredefinedClassificationTypeNames.String) }
			};
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged
		{
			add { }
			remove { }
		}

		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			foreach (var tagSpan in aggregator.GetTags(spans))
			{
				// I've no idea why we get a collection here, then use the first item; this is how it was done in OokClassifier. It does
				// appear to only ever have one item; but I wonder if that's because we don't hae overlapping tags? Might need more
				// investigation if this every breaks/becomes flaky.
				var tagSpans = tagSpan.Span.GetSpans(buffer.CurrentSnapshot);
				if (classificationMapping.ContainsKey(tagSpan.Tag.Type))
					yield return new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(classificationMapping[tagSpan.Tag.Type]));
			}
		}
	}
}
