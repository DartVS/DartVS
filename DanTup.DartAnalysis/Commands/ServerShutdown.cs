using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	public static class ServerShutdownImplementation
	{
		public static async Task ServerShutdown(this DartAnalysisService service)
		{
			var response = await service.Service
				.Send(new ServerShutdown())
				.ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
