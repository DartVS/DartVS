using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
using FluentAssertions;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class CompletionTests : Tests
	{
		[Fact]
		public async Task Completion1()
		{
			var codeTemplate = @"
				main() {
					var name = ""Danny"";
					print(name.[[[t|||ab]]]);
				}
			";

			var expectedReplacements = new[] {
				"codeUnitAt",
				"startsWith",
				"endsWith",
				"split",
			};

			await PerformCompletionTest(codeTemplate, expectedReplacements);
		}

		[Fact]
		public async Task Completion2()
		{
			var codeTemplate = @"
				main() {
					[[[m|||]]]
				}
			";

			var expectedReplacements = new[] {
				"main"
			};

			await PerformCompletionTest(codeTemplate, expectedReplacements);
		}


		public async Task PerformCompletionTest(string fileTemplate, string[] expectedReplacements)
		{
			/// HACK:
			/// Code template shoud have the expected replacement area surrounded by [[[ brackets ]]]
			/// Cursor location to invoke completion for marked with |||
			/// Hopefully easier than counting characters!

			var fileContents = fileTemplate.Replace("[[[", "").Replace("|||", "").Replace("]]]", "");
			var expectedReplacementOffset = fileTemplate.IndexOf("[[[");
			var cursorLocation = fileTemplate.Replace("[[[", "").IndexOf("|||");
			var expectedReplacementLength = fileTemplate.Replace("[[[", "").Replace("|||", "").IndexOf("]]]") - expectedReplacementOffset;


			using (var service = CreateTestService())
			{
				// Set the roots to our known project and wait for the analysis to complete.
				await service.SetAnalysisRoots(new[] { SampleDartProject });
				await service.WaitForAnalysis();

				// Update the content of a file to the test-provided content.
				await service.UpdateContent(HelloWorldFile, new AddContentOverlay { Type = "add", Content = fileContents });

				// Kick off a request to start fetching suggestions.
				var completionID = await service.GetSuggestions(HelloWorldFile, cursorLocation);

				// Subcsribe to suggestion notifications that match the ID we were previously given back.
				var result = await service.CompletionResultsNotification.Where(cr => cr.Id == completionID && cr.IsLast).FirstAsync();

				// Check expected results.
				result.Id.Should().Be(completionID);
				result.IsLast.Should().BeTrue();
				result.ReplacementOffset.Should().Be(expectedReplacementOffset);
				result.ReplacementLength.Should().Be(expectedReplacementLength);
				result.Results.Select(r => r.Completion).Should().Contain(expectedReplacements);

			}
		}
	}
}
