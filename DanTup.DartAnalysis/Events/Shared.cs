using System;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct AnalysisElement
	{
		[JsonProperty]
		public ElementKind Kind { get; internal set; }

		[JsonProperty]
		public string Name { get; internal set; }

		[JsonProperty]
		public AnalysisLocation Location { get; internal set; }

		[JsonProperty]
		public AnalysisElementFlags Flags { get; internal set; }

		[JsonProperty]
		public string Parameters { get; internal set; }

		[JsonProperty]
		public string ReturnType { get; internal set; }

		[JsonProperty]
		public AnalysisOutline[] Children { get; internal set; }
	}

	public struct AnalysisLocation
	{
		[JsonProperty]
		public string File { get; internal set; }

		[JsonProperty]
		public int Offset { get; internal set; }

		[JsonProperty]
		public int Length { get; internal set; }

		[JsonProperty]
		public int StartLine { get; internal set; }

		[JsonProperty]
		public int StartColumn { get; internal set; }
	}

	public enum ElementKind
	{
		Class,
		ClassTypeAlias,
		CompilationUnit,
		Constructor,
		Field,
		Function,
		FunctionTypeAlias,
		Getter,
		Library,
		LocalVariable,
		Method,
		Parameter,
		Setter,
		TopLevelVariable,
		TypeParameter,
		UnitTestCase,
		UnitTestGroup,
		Unknown,
	}

	[Flags]
	public enum AnalysisElementFlags
	{
		None,
		Abstract = 0x01,
		Constant = 0x02,
		Deprecated = 0x20,
		Final = 0x04,
		Private = 0x10,
		Static = 0x08,
	}
}
