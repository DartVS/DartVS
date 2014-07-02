using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisNavigationEventJson
	{
		public string file = null;
		public AnalysisNavigationRegionJson[] regions = null;
	}

	class AnalysisNavigationRegionJson
	{
		public int offset = 0;
		public int length = 0;
		public AnalysisNavigationTargetJson[] targets = null;
	}

	class AnalysisNavigationTargetJson
	{
		public string file = null;
		public int offset = 0;
		public int length = 0;
		public AnalysisElementJson element = null;
	}

	#endregion

	public struct AnalysisNavigationNotification
	{
		public string File { get; internal set; }
		public AnalysisNavigationRegion[] Regions { get; internal set; }
	}

	public struct AnalysisNavigationRegion
	{
		public int Offset { get; internal set; }
		public int Length { get; internal set; }
		public AnalysisNavigationTarget[] Targets { get; internal set; }
	}

	public struct AnalysisNavigationTarget
	{
		public string File;
		public int Offset;
		public int Length;
		public AnalysisElement Element;
	}

	internal static class AnalysisNavigationEventImplementation
	{
		static ElementKind[] ElementKinds = Enum.GetValues(typeof(ElementKind)).Cast<ElementKind>().ToArray();

		public static AnalysisNavigationNotification AsNotification(this AnalysisNavigationEventJson notification)
		{
			return new AnalysisNavigationNotification
			{
				File = notification.file,
				Regions = notification.regions.Select(e => new AnalysisNavigationRegion
				{
					Offset = e.offset,
					Length = e.length,
					Targets = e.targets.Select(t => new AnalysisNavigationTarget
					{
						File = t.file,
						Offset = t.offset,
						Length = t.length,
						Element = new AnalysisElement
						{
							Kind = ElementKinds.FirstOrDefault(ek => ek.ToString().ToLowerInvariant() == t.element.kind.ToLowerInvariant().Replace("_", "")),
							Name = t.element.name,
							Offset = t.element.offset,
							Length = t.element.length,
							Flags = (AnalysisElementFlags)t.element.flags,
							Parameters = t.element.parameters,
							ReturnType = t.element.returnType,
						}
					}).ToArray()
				}).ToArray(),
			};
		}
	}
}
