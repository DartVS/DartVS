using System;
using System.Threading.Tasks;
namespace DanTup.DartAnalysis
{
	class ServerVersionRequest : Request<Response<ServerVersionResponse>>
	{
		public string method = "server.getVersion";
	}

	class ServerVersionResponse
	{
		public string version = null;
	}

	public static class ServerVersionRequestImplementation
	{
		public static async Task<Version> GetServerVersion(this DartAnalysisService service)
		{
			var response = await service.Service.Send(new ServerVersionRequest());

			return Version.Parse(response.result.version);
		}
	}
}
