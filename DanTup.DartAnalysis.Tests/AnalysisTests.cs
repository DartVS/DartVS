using System.Threading.Tasks;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class AnalysisTests : Tests
	{
		[Fact(Skip = "Incomplete test; requires event implementation")]
		public async Task SetAnalysisRoots()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				await service.SetAnalysisRoots(new[] { SampleDartProject }, new string[] { });

				// TODO: Wait for some events!
			}
		}
	}
}
