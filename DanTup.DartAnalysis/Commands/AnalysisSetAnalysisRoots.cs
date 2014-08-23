using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class AnalysisSetAnalysisRootsImplementation
	{
		public static Task SetAnalysisRoots(this DartAnalysisService service, string[] included)
		{
			return service.SetAnalysisRoots(included, new string[0]);
		}

		public static async Task SetAnalysisRoots(this DartAnalysisService service, string[] included, string[] excluded)
		{
			var request = new AnalysisSetAnalysisRootsRequest
			{
				Included = included,
				Excluded = excluded
			};

			var response = await service.Service
				.Send(new AnalysisSetAnalysisRoots(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
