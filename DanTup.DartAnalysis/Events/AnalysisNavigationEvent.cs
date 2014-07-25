using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct AnalysisNavigationEvent
	{
		[JsonProperty]
		public string File { get; internal set; }

		[JsonProperty]
		public AnalysisNavigationRegion[] Regions { get; internal set; }
	}

	public struct AnalysisNavigationRegion
	{
		[JsonProperty]
		public int Offset { get; internal set; }

		[JsonProperty]
		public int Length { get; internal set; }

		[JsonProperty]
		public AnalysisElement[] Targets { get; internal set; }
	}
}
