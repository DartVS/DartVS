using System.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class ServerSetSubscriptionImplementation
	{
		public static async Task SetServerSubscriptions(this DartAnalysisService service, ServerService[] subscriptions)
		{
			var request = new ServerSetSubscriptionsRequest
			{
				Subscriptions = subscriptions
			};

			var response = await service
				.Service.Send(new ServerSetSubscriptions(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
