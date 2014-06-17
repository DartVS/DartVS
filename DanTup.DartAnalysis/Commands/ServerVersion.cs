using System;
using System.Threading.Tasks;
namespace DanTup.DartAnalysis
{
	class VersionRequest : Request<Response<VersionResponse>>
	{
		public string method = "server.getVersion";
	}

	class VersionResponse
	{
		public string version = null;
	}

	public static class VersionRequestImplementation
	{
		public static async Task<Version> GetServerVersion(this DartAnalysisService service)
		{
			var response = await service.Service.Send(new VersionRequest());

			return Version.Parse(response.result.version);
		}
	}
}
