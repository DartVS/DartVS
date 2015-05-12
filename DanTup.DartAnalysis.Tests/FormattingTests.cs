using System.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
using FluentAssertions;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class FormattingTests : Tests
	{
		// TODO: Clean up tests; map over to FluentAssertions

		[Fact]
		public async Task FormatText()
		{
			const string text = @"main()    
   {
print('test');
	}";
			const string expectedText = @"main() {
  print('test');
}
";

			using (var service = CreateTestService())
			{
				// Set the roots to our known project and wait for the analysis to complete.
				await service.SetAnalysisRoots(new[] { SampleDartProject });
				await service.WaitForAnalysis();

				//var edit = new SourceEdit { Replacement = text, Offset = 1, Length = 2 }; // TODO: Set this to whole file..
				await service.UpdateContent(HelloWorldFile, new AddContentOverlay { Type = "add", Content = text });
				var results = await service.Format(HelloWorldFile, 1, 1);

				results.Edits.Should().HaveCount(1);
				results.Edits.Single().Replacement.Should().Be(expectedText);
				results.Edits.Single().Offset.Should().Be(0);
				results.Edits.Single().Length.Should().Be(36);
				results.SelectionOffset.Should().Be(1);
				results.SelectionLength.Should().Be(1);
			}
		}
	}
}
