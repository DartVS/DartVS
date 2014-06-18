using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisOutlineEvent
	{
		public string file = null;
		public AnalysisOutlineDetails outline = null;
	}

	class AnalysisOutlineDetails
	{
		public string kind = null;
		public string name = null;
		public int nameOffset = 0;
		public int nameLength = 0;
		public int elementOffset = 0;
		public int elementLength = 0;
		public bool isAbstract = false;
		public bool isStatic = false;
		public AnalysisOutlineDetails[] children = null;
	}

	#endregion

	public struct AnalysisOutlineNotification
	{
		public string File { get; internal set; }
		public AnalysisOutline Outline { get; internal set; }
	}

	public struct AnalysisOutline
	{
		public ElementKind Kind { get; internal set; }
		public string Name { get; internal set; }
		public int NameOffset { get; internal set; }
		public int NameLength { get; internal set; }
		public int ElementOffset { get; internal set; }
		public int ElementLength { get; internal set; }
		public bool IsAbstract { get; internal set; }
		public bool IsStatic { get; internal set; }
		public AnalysisOutline[] Children { get; internal set; }
	}

	public enum ElementKind
	{
		Class,
		ClassTypeAlias,
		CompilationUnit,
		Constructor,
		Getter,
		Field,
		Function,
		FunctionTypeAlias,
		Library,
		Method,
		Setter,
		TopLevelVariable,
		Unknown,
		UnitTestCase,
		UnitTestGroup
	}

	internal static class AnalysisOutlineEventImplementation
	{
		static ElementKind[] ElementKinds = Enum.GetValues(typeof(ElementKind)).Cast<ElementKind>().ToArray();

		public static AnalysisOutlineNotification AsNotification(this AnalysisOutlineEvent notification)
		{
			Func<AnalysisOutlineDetails, AnalysisOutline> convert = null;
			convert = o => new AnalysisOutline
				{
					Kind = ElementKinds.FirstOrDefault(ek => ek.ToString().ToLowerInvariant() == o.kind.ToLowerInvariant().Replace("_", "")),
					Name = o.name,
					NameOffset = o.nameOffset,
					NameLength = o.nameLength,
					ElementOffset = o.elementOffset,
					ElementLength = o.elementLength,
					IsAbstract = o.isAbstract,
					IsStatic = o.isStatic,
					Children = o.children == null ? null : o.children.Select(convert).ToArray()
				};

			return new AnalysisOutlineNotification
			{
				File = notification.file,
				Outline = convert(notification.outline),
			};
		}
	}
}
