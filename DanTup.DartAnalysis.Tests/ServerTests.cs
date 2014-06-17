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

		[Fact(Skip = "Server shutdown doesn't seem to work yet?")]
		public async Task ServerShutdown()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				await service.ServerShutdown();

				// Attempt to send a normal request; this should fail because the server has been shutdown!
				Assert.Throws<Exception>(() => service.GetServerVersion().Wait());
			}
		}
	}
}
