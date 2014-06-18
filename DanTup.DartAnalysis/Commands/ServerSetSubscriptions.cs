using System.Threading.Tasks;
using System.Linq;

namespace DanTup.DartAnalysis
{
	class ServerSetSubscriptionsRequest : Request<ServerSetSubscriptionParams, Response>
	{
		public string method = "server.setSubscriptions";

		public ServerSetSubscriptionsRequest(params string[] subscriptions)
		{
			this.@params = new ServerSetSubscriptionParams(subscriptions);
		}
	}

	class ServerSetSubscriptionParams
	{
		public string[] subscriptions;

		public ServerSetSubscriptionParams(params string[] subscriptions)
		{
			this.subscriptions = subscriptions;
		}
	}

	public static class ServerSetSubscriptionImplementation
	{
		public static async Task SetServerSubscriptions(this DartAnalysisService service, params ServerSubscription[] subscriptions)
		{
			var response = await service.Service.Send(new ServerSetSubscriptionsRequest(subscriptions.Select(s => s.ToString().ToUpperInvariant()).ToArray()));

			// There's nothing useful on this response to return.

			return;
		}
	}

	public enum ServerSubscription
	{
		Status
	}
}
