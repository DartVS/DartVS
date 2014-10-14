using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
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
		IDictionary<HighlightRegionType, IClassificationType> classificationMapping;

		public ClassificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisServiceFactory analysisServiceFactory, IClassificationTypeRegistryService typeService)
			: base(buffer, textDocumentFactory, analysisServiceFactory)
		{
			// Mapping of Analysis Server's HighlightRegionTypes to the VS Classifications we wish to give them.
			classificationMapping = new Dictionary<HighlightRegionType, IClassificationType>
			{
				// TODO: Replace any 'Other' with something more appropriate (eg. NaturalLanguage/FormatLanguage or more specific).
				// NOTE: We must map *everything* here, because Tooltips rely on these tags to calculate applicableTo spans immediately.
				{ HighlightRegionType.Annotation, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightRegionType.BuiltIn, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.Class, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightRegionType.CommentBlock, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.CommentDocumentation, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.CommentEndOfLine, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.Constructor, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Directive, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.DynamicType, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Field, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.FieldStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Function, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.FunctionDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.FunctionTypeAlias, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.GetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.IdentifierDefault, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.ImportPrefix, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Keyword, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.LiteralBoolean, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.LiteralDouble, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				{ HighlightRegionType.LiteralInteger, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				{ HighlightRegionType.LiteralList, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.LiteralMap, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.LiteralString, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.LocalVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.LocalVariableDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Method, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.MethodDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.MethodDeclarationStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.MethodStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.Parameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.SetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.TopLevelVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.TypeNameDynamic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
				{ HighlightRegionType.TypeParameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Other) },
			};

			this.Subscribe();
		}

		protected override ITagSpan<ClassificationTag> CreateTag(HighlightRegion highlight)
		{
			return new TagSpan<ClassificationTag>(new SnapshotSpan(buffer.CurrentSnapshot, highlight.Offset, highlight.Length), new ClassificationTag(classificationMapping[highlight.Type]));
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

			return notification.Regions.Where(h => classificationMapping.ContainsKey(h.Type)).ToArray();
		}

		protected override Tuple<int, int> GetOffsetAndLength(HighlightRegion data)
		{
			return Tuple.Create(data.Offset, data.Length);
		}
	}
}
