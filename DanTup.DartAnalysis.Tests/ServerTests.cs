﻿using System;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class ServerTests : Tests
	{
		[Fact]
		public async Task ServerVersion()
		{
			using (var service = CreateTestService())
			{
				var version = await service.GetServerVersion();

				Assert.Equal(new Version(0, 0, 1), version);
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
				Assert.Equal("Expected parameter subscriptions to be a list of names from the list [STATUS]", err.ErrorMessage);
			}
		}
	}
}
