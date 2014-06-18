using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	class AnalysisSetSubscriptionsRequest : Request<AnalysisSetSubscriptionParams, Response>
	{
		public string method = "analysis.setSubscriptions";

		public AnalysisSetSubscriptionsRequest(Dictionary<string, string[]> subscriptions)
		{
			this.@params = new AnalysisSetSubscriptionParams(subscriptions);
		}
	}

	class AnalysisSetSubscriptionParams
	{
		public Dictionary<string, string[]> subscriptions;

		public AnalysisSetSubscriptionParams(Dictionary<string, string[]> subscriptions)
		{
			this.subscriptions = subscriptions;
		}
	}

	public static class AnalysisSetSubscriptionImplementation
	{
		public static Task SetAnalysisSubscriptions(this DartAnalysisService service, string[] subscriptions, string root)
		{
			return service.SetAnalysisSubscriptions(subscriptions.ToDictionary(s => s, s => new[] { root }));
		}

		public static async Task SetAnalysisSubscriptions(this DartAnalysisService service, Dictionary<string, string[]> subscriptions)
		{
			var response = await service.Service.Send(new AnalysisSetSubscriptionsRequest(subscriptions));

			// There's nothing useful on this response to return.

			return;
		}
	}
}
