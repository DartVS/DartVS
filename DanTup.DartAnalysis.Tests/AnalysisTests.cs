using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class AnalysisTests : Tests
	{
		[Fact]
		public async Task SetAnalysisRoots()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				// Keep track of when we're called to say server status has changed
				bool? isAnalyzing = null;
				service.ServerStatusNotification += (s, e) => { isAnalyzing = e.IsAnalysing; };

				// Keep track of errors that are reported
				List<AnalysisError> errors = new List<AnalysisError>();
				service.AnalysisErrorsNotification += (s, e) => { errors.AddRange(e.Errors); };


				// Send a request to do some analysis.
				await service.SetAnalysisRoots(new[] { SampleDartProject }, new string[] { });

				// Allow time for analysing.
				await Task.Delay(5000);


				// Ensure the event fired to say analysing had finished.
				Assert.Equal(false, isAnalyzing);

				// Ensure the error-free file got no errors.
				Assert.Equal(0, errors.Where(e => e.File.EndsWith("\\hello_world.dart")).Count());

				// Ensure the single-error file got the expected error.
				Assert.Equal(1, errors.Where(e => e.File.EndsWith("\\single_type_error.dart")).Distinct().Count());
				var error = errors.First(e => e.File.EndsWith("\\single_type_error.dart"));
				Assert.Equal("StaticWarningCode.ARGUMENT_TYPE_NOT_ASSIGNABLE", error.ErrorCode);
			}
		}
	}
}
