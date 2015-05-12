using System;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class ServerTests : Tests
	{
		// TODO: Clean up tests; map over to FluentAssertions

		[Fact]
		public async Task ServerVersion()
		{
			using (var service = CreateTestService())
			{
				var version = await service.GetServerVersion();

				Assert.Equal(new Version(1, 6, 0), version);
			}
		}

		[Fact]
		public async Task ServerShutdown()
		{
			using (var service = CreateTestService())
			{
				await service.ServerShutdown();

				await Task.Delay(5000); // Allow 5 secs for shutdown

				// Attempt to send a normal request; this should fail because the server has been shutdown!
				Assert.Throws<AggregateException>(() => service.GetServerVersion().Wait());
			}
		}

		[Fact]
		public async Task ServerSetSubscriptions()
		{
			using (var service = CreateTestService())
			{
				await service.SetServerSubscriptions(new[] { ServerService.Status });
			}
		}

		[Fact]
		public void BadRequestFails()
		{
			// Send a bad request (Server SetSubscription with bad subscription value) and check we
			// get the correct type of response.
			using (var service = CreateTestService())
			{
				var ex = Assert.Throws<AggregateException>(() => service.SetServerSubscriptions(new[] { (ServerService)1234 }).Wait());
				Assert.Equal(1, ex.InnerExceptions.Count);
				Assert.IsType<ErrorResponseException>(ex.GetBaseException());
				var err = ex.GetBaseException() as ErrorResponseException;
				Assert.Equal("INVALID_PARAMETER", err.Code);

				// This error is bad, but what the Analysis Service currently returns!
				// https://groups.google.com/a/dartlang.org/forum/#!topic/analyzer-discuss/TwIcHnx8rR4
				Assert.Equal("Invalid parameter 'params.subscriptions[0]'. be ServerService.", err.ErrorMessage);
				//Assert.Equal("Expected parameter params.subscriptions[0] to be ServerService", err.ErrorMessage);
			}
		}
	}
}
