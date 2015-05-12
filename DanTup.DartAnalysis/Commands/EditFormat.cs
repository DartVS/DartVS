using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class EditFormatImplementation
	{
		public static async Task<EditFormatResponse> Format(this DartAnalysisService service, string file, int offset, int length)
		{
			var request = new EditFormatRequest
			{
				File = file,
				SelectionOffset = offset,
				SelectionLength = length
			};

			var response = await service.Service
				.Send(new EditFormat(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			return response.result;
		}
	}
}
