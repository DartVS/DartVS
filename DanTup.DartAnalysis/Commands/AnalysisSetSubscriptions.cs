using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class AnalysisSetSubscriptionImplementation
	{
		public static Task SetAnalysisSubscriptions(this DartAnalysisService service, AnalysisService[] subscriptions, string root)
		{
			return service.SetAnalysisSubscriptions(subscriptions.ToDictionary(s => s, s => new[] { root }));
		}

		public static async Task SetAnalysisSubscriptions(this DartAnalysisService service, Dictionary<AnalysisService, string[]> subscriptions)
		{
			var request = new AnalysisSetSubscriptionsRequest
			{
				Subscriptions = subscriptions
			};

			var response = await service.Service
				.Send(new AnalysisSetSubscriptions(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
