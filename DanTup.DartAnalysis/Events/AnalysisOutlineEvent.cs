using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisOutlineEventJson
	{
		public string file = null;
		public AnalysisOutlineJson outline = null;
	}

	class AnalysisOutlineJson
	{
		public AnalysisElementJson element = null;
		public int offset = 0;
		public int length = 0;
		public AnalysisOutlineJson[] children = null;
	}

	#endregion

	public struct AnalysisOutlineNotification
	{
		public string File { get; internal set; }
		public AnalysisOutline Outline { get; internal set; }
	}

	public struct AnalysisOutline
	{
		public AnalysisElement Element { get; internal set; }
		public int Offset { get; internal set; }
		public int Length { get; internal set; }
		public AnalysisOutline[] Children { get; internal set; }
	}

	internal static class AnalysisOutlineEventImplementation
	{
		static ElementKind[] ElementKinds = Enum.GetValues(typeof(ElementKind)).Cast<ElementKind>().ToArray();

		public static AnalysisOutlineNotification AsNotification(this AnalysisOutlineEventJson notification)
		{
			Func<AnalysisOutlineJson, AnalysisOutline> convert = null;
			convert = o => new AnalysisOutline
				{
					Element = new AnalysisElement
					{
						Kind = ElementKinds.FirstOrDefault(ek => ek.ToString().ToLowerInvariant() == o.element.kind.ToLowerInvariant().Replace("_", "")),
						Name = o.element.name,
						Offset = o.element.offset,
						Length = o.element.length,
						Flags = (AnalysisElementFlags)o.element.flags,
						Parameters = o.element.parameters,
						ReturnType = o.element.returnType,
					},
					Offset = o.offset,
					Length = o.length,
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
