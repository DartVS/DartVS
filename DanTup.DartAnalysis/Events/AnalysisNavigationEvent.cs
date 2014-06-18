using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisNavigationEvent
	{
		public string file = null;
		public AnalysisNavigationRegionDetails[] regions = null;
	}

	class AnalysisNavigationRegionDetails
	{
		public int offset = 0;
		public int length = 0;
		public AnalysisNavigationTargetDetails[] targets = null;
	}

	class AnalysisNavigationTargetDetails
	{
		public string file = null;
		public int offset = 0;
		public int length = 0;
		public string elementId = null;
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
		public string ElementID;
	}

	internal static class AnalysisNavigationEventImplementation
	{
		public static AnalysisNavigationNotification AsNotification(this AnalysisNavigationEvent notification)
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
						ElementID = t.elementId
					}).ToArray()
				}).ToArray(),
			};
		}
	}
}
