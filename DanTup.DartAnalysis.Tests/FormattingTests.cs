using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class FormattingTests : Tests
	{
		// TODO: Clean up tests; map over to FluentAssertions

		[Fact]
		public void FormatText()
		{
			using (var service = new DartFormatter(SdkFolder))
			{
				var text = @"main()    
   {
print('test');
	}";
				var expectedText = @"main() {
  print('test');
}
";

				var formattedText = service.FormatText(text);

				Assert.Equal(expectedText, formattedText);
			}
		}
	}
}
