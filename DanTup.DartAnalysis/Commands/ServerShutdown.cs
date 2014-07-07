using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	class ServerShutdownRequest : Request<Response>
	{
		public string method = "server.shutdown";
	}

	public static class ServerShutdownImplementation
	{
		public static async Task ServerShutdown(this DartAnalysisService service)
		{
			var response = await service.Service.Send(new ServerShutdownRequest()).ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
