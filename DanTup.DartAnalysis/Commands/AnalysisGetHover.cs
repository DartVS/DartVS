using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class AnalysisGetHoverImplementation
	{
		public static async Task<HoverInformation[]> GetHover(this DartAnalysisService service, string file, int offset)
		{
			var request = new AnalysisGetHoverRequest
			{
				File = file,
				Offset = offset
			};

			var response = await service.Service
				.Send(new AnalysisGetHover(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			return response.result.Hovers;
		}
	}
}
