using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using DanTup.DartAnalysis;
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

	class ClassificationTagger : AnalysisNotificationTagger<ClassificationTag, AnalysisHighlightRegion, AnalysisHighlightsEvent>
	{
		IDictionary<HighlightType, IClassificationType> classificationMapping;

		public ClassificationTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService, IClassificationTypeRegistryService typeService)
			: base(buffer, textDocumentFactory, analysisService)
		{
			// Mapping of Analysis Server's HighlightTypes to the VS Classifications we wish to give them.
			classificationMapping = new Dictionary<HighlightType, IClassificationType>
			{
				// TODO: Fill all this in!
				{ HighlightType.Annotation, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightType.BuiltIn, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightType.Class, typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference) },
				{ HighlightType.CommentBlock, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightType.CommentDocumentation, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightType.CommentEndOfLine, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Constructor, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Directive, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.DynamicType, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Field, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.FieldStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Function, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.FunctionDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.FunctionTypeAlias, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.GetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.IdentifierDefault, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.ImportPrefix, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				{ HighlightType.Keyword, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightType.LiteralBoolean, typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword) },
				{ HighlightType.LiteralDouble, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				{ HighlightType.LiteralInteger, typeService.GetClassificationType(PredefinedClassificationTypeNames.Number) },
				//{ HighlightType.LiteralList, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.LiteralMap, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.LiteralString, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.LocalVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.LocalVariableDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Method, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.MethodDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.MethodDeclarationStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.MethodStatic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.Parameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.SetterDeclaration, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.TopLevelVariable, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.TypeNameDynamic, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
				//{ HighlightType.TypeParameter, typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment) },
			};

			this.Subscribe();
		}

		protected override ITagSpan<ClassificationTag> CreateTag(AnalysisHighlightRegion highlight)
		{
			return new TagSpan<ClassificationTag>(new SnapshotSpan(buffer.CurrentSnapshot, highlight.Offset, highlight.Length), new ClassificationTag(classificationMapping[highlight.Type]));
		}

		protected override IDisposable Subscribe(Action<AnalysisHighlightsEvent> updateSourceData)
		{
			return this.analysisService.AnalysisHighlightsNotification.Where(en => en.File == textDocument.FilePath).Subscribe(updateSourceData);
		}

		protected override AnalysisHighlightRegion[] GetDataToTag(AnalysisHighlightsEvent notification)
		{
			// HACK: This happens if when the base constructor Subscribes, we already ahve an event in the stream...
			// TODO: Make this better. Ideally, we need to set classificationMapping before the base class Subscribes...
			if (classificationMapping == null)
				return new AnalysisHighlightRegion[0];

			return notification.Regions.Where(h => classificationMapping.ContainsKey(h.Type)).ToArray();
		}

		protected override Tuple<int, int> GetOffsetAndLength(AnalysisHighlightRegion data)
		{
			return Tuple.Create(data.Offset, data.Length);
		}
	}
}
