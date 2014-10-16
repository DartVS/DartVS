using System.ComponentModel.Composition;
using DanTup.DartAnalysis.Json;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS.Taggers
{
	internal static class DartClassificationTypes
	{
		[Export]
		[ClassificationTypeName(HighlightRegionType.Annotation)]
		[BaseDefinition(PredefinedClassificationTypeNames.SymbolReference)]
		private static readonly ClassificationTypeDefinition Annotation;

		[Export]
		[ClassificationTypeName(HighlightRegionType.BuiltIn)]
		[BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
		private static readonly ClassificationTypeDefinition BuiltIn;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Class)]
		[BaseDefinition(PredefinedClassificationTypeNames.SymbolReference)]
		private static readonly ClassificationTypeDefinition Class;

		[Export]
		[ClassificationTypeName(HighlightRegionType.CommentBlock)]
		[BaseDefinition(PredefinedClassificationTypeNames.Comment)]
		private static readonly ClassificationTypeDefinition CommentBlock;

		[Export]
		[ClassificationTypeName(HighlightRegionType.CommentDocumentation)]
		[BaseDefinition(PredefinedClassificationTypeNames.Comment)]
		private static readonly ClassificationTypeDefinition CommentDocumentation;

		[Export]
		[ClassificationTypeName(HighlightRegionType.CommentEndOfLine)]
		[BaseDefinition(PredefinedClassificationTypeNames.Comment)]
		private static readonly ClassificationTypeDefinition CommentEndOfLine;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Constructor)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Constructor;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Directive)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Directive;

		[Export]
		[ClassificationTypeName(HighlightRegionType.DynamicType)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition DynamicType;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Field)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Field;

		[Export]
		[ClassificationTypeName(HighlightRegionType.FieldStatic)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition FieldStatic;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Function)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Function;

		[Export]
		[ClassificationTypeName(HighlightRegionType.FunctionDeclaration)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition FunctionDeclaration;

		[Export]
		[ClassificationTypeName(HighlightRegionType.FunctionTypeAlias)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition FunctionTypeAlias;

		[Export]
		[ClassificationTypeName(HighlightRegionType.GetterDeclaration)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition GetterDeclaration;

		[Export]
		[ClassificationTypeName(HighlightRegionType.IdentifierDefault)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition IdentifierDefault;

		[Export]
		[ClassificationTypeName(HighlightRegionType.ImportPrefix)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition ImportPrefix;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Keyword)]
		[BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
		private static readonly ClassificationTypeDefinition Keyword;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Label)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Label;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralBoolean)]
		[BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
		private static readonly ClassificationTypeDefinition LiteralBoolean;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralDouble)]
		[BaseDefinition(PredefinedClassificationTypeNames.Number)]
		private static readonly ClassificationTypeDefinition LiteralDouble;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralInteger)]
		[BaseDefinition(PredefinedClassificationTypeNames.Number)]
		private static readonly ClassificationTypeDefinition LiteralInteger;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralList)]
		[BaseDefinition(PredefinedClassificationTypeNames.Other)]
		private static readonly ClassificationTypeDefinition LiteralList;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralMap)]
		[BaseDefinition(PredefinedClassificationTypeNames.Other)]
		private static readonly ClassificationTypeDefinition LiteralMap;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LiteralString)]
		[BaseDefinition(PredefinedClassificationTypeNames.String)]
		private static readonly ClassificationTypeDefinition LiteralString;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LocalVariable)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition LocalVariable;

		[Export]
		[ClassificationTypeName(HighlightRegionType.LocalVariableDeclaration)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition LocalVariableDeclaration;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Method)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Method;

		[Export]
		[ClassificationTypeName(HighlightRegionType.MethodDeclaration)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition MethodDeclaration;

		[Export]
		[ClassificationTypeName(HighlightRegionType.MethodDeclarationStatic)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition MethodDeclarationStatic;

		[Export]
		[ClassificationTypeName(HighlightRegionType.MethodStatic)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition MethodStatic;

		[Export]
		[ClassificationTypeName(HighlightRegionType.Parameter)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition Parameter;

		[Export]
		[ClassificationTypeName(HighlightRegionType.SetterDeclaration)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition SetterDeclaration;

		[Export]
		[ClassificationTypeName(HighlightRegionType.TopLevelVariable)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition TopLevelVariable;

		[Export]
		[ClassificationTypeName(HighlightRegionType.TypeNameDynamic)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition TypeNameDynamic;

		[Export]
		[ClassificationTypeName(HighlightRegionType.TypeParameter)]
		[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
		private static readonly ClassificationTypeDefinition TypeParameter;

		private sealed class ClassificationTypeNameAttribute : SingletonBaseMetadataAttribute
		{
			private readonly string _name;

			public ClassificationTypeNameAttribute(HighlightRegionType highlightRegionType)
			{
				_name = DartConstants.ContentType + highlightRegionType.ToString();
			}

			public string Name
			{
				get
				{
					return _name;
				}
			}
		}
	}
}
