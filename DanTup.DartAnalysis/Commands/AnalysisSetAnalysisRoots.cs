using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	class AnalysisSetAnalysisRootsRequest : Request<AnalysisSetAnalysisRootsParams, Response>
	{
		public string method = "analysis.setAnalysisRoots";

		public AnalysisSetAnalysisRootsRequest(string[] included, string[] excluded)
		{
			this.@params = new AnalysisSetAnalysisRootsParams(included, excluded);
		}
	}

	class AnalysisSetAnalysisRootsParams
	{
		public string[] included;
		public string[] excluded;

		public AnalysisSetAnalysisRootsParams(string[] included, string[] excluded)
		{
			this.included = included;
			this.excluded = excluded;
		}
	}

	public static class AnalysisSetAnalysisRootsImplementation
	{
		public static async Task SetAnalysisRoots(this DartAnalysisService service, string[] included, string[] excluded)
		{
			var response = await service.Service.Send(new AnalysisSetAnalysisRootsRequest(included, excluded));

			// There's nothing useful on this response to return.

			return;
		}
	}
}
