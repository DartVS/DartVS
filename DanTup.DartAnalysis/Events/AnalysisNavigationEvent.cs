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
		public AnalysisElementJson[] targets = null;
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
		public AnalysisElement[] Targets { get; internal set; }
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
					Targets = e.targets.Select(t => new AnalysisElement
					{
						Kind = ElementKinds.FirstOrDefault(ek => ek.ToString().ToLowerInvariant() == t.kind.ToLowerInvariant().Replace("_", "")),
						Name = t.name,
						Location = new AnalysisLocation
						{
							File = t.location.file,
							Offset = t.location.offset,
							Length = t.location.length,
							StartLine = t.location.startLine,
							StartColumn = t.location.startColumn,
						},
						Flags = (AnalysisElementFlags)t.flags,
						Parameters = t.parameters,
						ReturnType = t.returnType,
					}).ToArray()
				}).ToArray(),
			};
		}
	}
}
