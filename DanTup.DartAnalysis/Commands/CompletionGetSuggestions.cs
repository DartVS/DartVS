using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class CompletionGetSuggestionsImplementation
	{
		public static async Task<string> GetSuggestions(this DartAnalysisService service, string file, int offset)
		{
			var request = new CompletionGetSuggestionsRequest
			{
				File = file,
				Offset = offset
			};

			var response = await service.Service
				.Send(new CompletionGetSuggestions(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			return response.result.Id;
		}
	}
}
