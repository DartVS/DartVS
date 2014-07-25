using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	public struct ServerStatusEvent
	{
		[JsonProperty]
		public bool IsAnalysing { get; internal set; }
	}
}
