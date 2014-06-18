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

		[Fact]
		public async Task ServerSetSubscriptions()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				await service.SetServerSubscriptions(ServerSubscription.Status);
			}
		}

		[Fact]
		public void BadRequestFails()
		{
			// Send a bad request (Server SetSubscription with bad subscription value) and check we
			// get the correct type of response.
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				var ex = Assert.Throws<AggregateException>(() => service.SetServerSubscriptions((ServerSubscription)1234).Wait());
				Assert.Equal(1, ex.InnerExceptions.Count);
				Assert.IsType<ErrorResponseException>(ex.GetBaseException());
				var err = ex.GetBaseException() as ErrorResponseException;
				Assert.Equal(-2, err.Code);
				Assert.Equal("Expected parameter subscriptions to be a list of names from the list [STATUS]", err.ErrorMessage);
			}
		}
	}
}
