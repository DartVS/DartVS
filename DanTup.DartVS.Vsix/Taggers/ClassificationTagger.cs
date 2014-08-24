using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
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
		DartAnalysisService analysisService = null;

		[Import]
		internal IClassificationTypeRegistryService classificationTypeRegistry = null;

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			return new ClassificationTagger(buffer, textDocumentFactory, analysisService, classificationTypeRegistry) as ITagger<T>;
		}
	}

	class ClassificationTagger : AnalysisNotificationTagger<ClassificationTag, HighlightRegion, AnalysisHighlightsNotification>
	{
		IDictionary<HighlightRegionType, IClassificationType> classificationMapping;

		public ClassificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService, IClassificationTypeRegistryService typeService)
			: base(buffer, textDocumentFactory, analysisService)
		{
			// Mapping of Analysis Server's HighlightRegionTypes to the VS Classifications we wish to give them.
			classificationMapping = new Dictionary<HighlightRegionType, IClassificationType>
			{
				// TODO: Fill all this in!
				{ HighlightRegionType.Annotation, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightRegionType.BuiltIn, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.Class, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightRegionType.CommentBlock, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.CommentDocumentation, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.CommentEndOfLine, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Constructor, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Directive, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.DynamicType, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Field, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.FieldStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Function, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.FunctionDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.FunctionTypeAlias, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.GetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.IdentifierDefault, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.ImportPrefix, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightRegionType.Keyword, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.LiteralBoolean, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightRegionType.LiteralDouble, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				{ HighlightRegionType.LiteralInteger, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				//{ HighlightRegionType.LiteralList, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.LiteralMap, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.LiteralString, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.LocalVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.LocalVariableDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Method, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.MethodDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.MethodDeclarationStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.MethodStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.Parameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.SetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.TopLevelVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.TypeNameDynamic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightRegionType.TypeParameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
			};

			this.Subscribe();
		}

		protected override ITagSpan<ClassificationTag> CreateTag(HighlightRegion highlight)
		{
			return new TagSpan<ClassificationTag>(new SnapshotSpan(buffer.CurrentSnapshot, highlight.Offset, highlight.Length), new ClassificationTag(classificationMapping[highlight.Type]));
		}

		protected override IDisposable Subscribe(Action<AnalysisHighlightsNotification> updateSourceData)
		{
			return this.analysisService.AnalysisHighlightsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(updateSourceData);
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
