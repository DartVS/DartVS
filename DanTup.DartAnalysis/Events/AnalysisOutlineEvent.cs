using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct AnalysisOutlineEvent
	{
		[JsonProperty]
		public string File { get; internal set; }

		[JsonProperty]
		public AnalysisOutline Outline { get; internal set; }
	}

	public struct AnalysisOutline
	{
		[JsonProperty]
		public AnalysisElement Element { get; internal set; }

		[JsonProperty]
		public int Offset { get; internal set; }

		[JsonProperty]
		public int Length { get; internal set; }

		[JsonProperty]
		public AnalysisOutline[] Children { get; internal set; }
	}
}
