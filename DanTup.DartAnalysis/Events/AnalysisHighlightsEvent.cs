using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisHighlightsEvent
	{
		public string file;
		public AnalysisHighlightRegionDetails[] regions;
	}

	class AnalysisHighlightRegionDetails
	{
		public int offset;
		public int length;
		public string type;
	}

	#endregion

	public struct AnalysisHighlightsNotification
	{
		public string File { get; internal set; }
		public AnalysisHighlightRegion[] Regions { get; internal set; }
	}

	public struct AnalysisHighlightRegion
	{
		public int Offset { get; internal set; }
		public int Length { get; internal set; }
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

	internal static class AnalysisHighlightsEventImplementation
	{
		static HighlightType[] HighlightTypes = Enum.GetValues(typeof(HighlightType)).Cast<HighlightType>().ToArray();

		public static AnalysisHighlightsNotification AsNotification(this AnalysisHighlightsEvent notification)
		{
			return new AnalysisHighlightsNotification
			{
				File = notification.file,
				Regions = notification.regions.Select(e => new AnalysisHighlightRegion
				{
					Offset = e.offset,
					Length = e.length,
					Type = HighlightTypes.FirstOrDefault(ht => ht.ToString().ToLowerInvariant() == e.type.ToLowerInvariant().Replace("_", "")),
				}).ToArray(),
			};
		}
	}
}
