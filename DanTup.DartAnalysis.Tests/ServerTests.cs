using System;
using System.Threading.Tasks;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class ServerTests : Tests
	{
		[Fact]
		public async Task ServerVersion()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				var version = await service.GetServerVersion();

				Assert.Equal(new Version(0, 0, 1), version);
			}
		}
	}
}
