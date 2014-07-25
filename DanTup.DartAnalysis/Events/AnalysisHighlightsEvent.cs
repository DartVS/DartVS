using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct AnalysisHighlightsEvent
	{
		[JsonProperty]
		public string File { get; internal set; }

		[JsonProperty]
		public AnalysisHighlightRegion[] Regions { get; internal set; }
	}

	public struct AnalysisHighlightRegion
	{
		[JsonProperty]
		public int Offset { get; internal set; }

		[JsonProperty]
		public int Length { get; internal set; }

		[JsonProperty]
		public HighlightType Type { get; internal set; }
	}

	public enum HighlightType
	{
		Annotation,
		BuiltIn,
		Class,
		CommentBlock,
		CommentDocumentation,
		CommentEndOfLine,
		Constructor,
		Directive,
		DynamicType,
		Field,
		FieldStatic,
		FunctionDeclaration,
		Function,
		FunctionTypeAlias,
		GetterDeclaration,
		Keyword,
		IdentifierDefault,
		ImportPrefix,
		LiteralBoolean,
		LiteralDouble,
		LiteralInteger,
		LiteralList,
		LiteralMap,
		LiteralString,
		LocalVariableDeclaration,
		LocalVariable,
		MethodDeclaration,
		MethodDeclarationStatic,
		Method,
		MethodStatic,
		Parameter,
		SetterDeclaration,
		TopLevelVariable,
		TypeNameDynamic,
		TypeParameter
	}
}
