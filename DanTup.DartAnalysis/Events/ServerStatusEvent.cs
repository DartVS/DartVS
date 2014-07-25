using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct ServerStatusEvent
	{
		[JsonProperty]
		public ServerAnalysisStatus Analysis { get; internal set; }
	}

	public struct ServerAnalysisStatus
	{
		[JsonProperty]
		public bool Analyzing { get; internal set; }
	}
}
