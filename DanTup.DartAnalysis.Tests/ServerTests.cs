using System.Threading.Tasks;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	class ServerTests : Tests
	{
		[Fact]
		public async Task ServerVersion()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
			}
		}
	}
}
