using System.Threading.Tasks;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class AnalysisTests : Tests
	{
		[Fact]
		public async Task SetAnalysisRoots()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				// Keep track of when we're called to say server status has changed
				bool? isAnalyzing = null;
				service.ServerStatusNotification += (s, e) => { isAnalyzing = e.IsAnalysing; };

				// Send a request to do some analysis.
				await service.SetAnalysisRoots(new[] { SampleDartProject }, new string[] { });

				// Allow time for analysing.
				await Task.Delay(5000);

				// Ensure the event fired to say analysing had finished.
				Assert.Equal(false, isAnalyzing);

				// TODO: Ensure expected errors came back...
			}
		}
	}
}
