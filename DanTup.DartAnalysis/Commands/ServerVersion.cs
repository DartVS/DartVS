using System;
using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	public static class ServerVersionRequestImplementation
	{
		public static async Task<Version> GetServerVersion(this DartAnalysisService service)
		{
			var response = await service.Service
				.Send(new ServerGetVersion())
				.ConfigureAwait(continueOnCapturedContext: false);

			return Version.Parse(response.result.Version);
		}
	}
}
