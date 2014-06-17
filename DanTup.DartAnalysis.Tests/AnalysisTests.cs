using System.Collections.Generic;
using System.IO;
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
				await service.SetAnalysisRoots(new[] { SampleDartProject });

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

		[Fact]
		public async Task TestAnalysisUpdateContent()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				// Keep track of errors that are reported.
				List<AnalysisError> errors = new List<AnalysisError>();
				service.AnalysisErrorsNotification += (s, e) => { errors.AddRange(e.Errors); };

				// Set the roots to our known project.
				await service.SetAnalysisRoots(new[] { SampleDartProject }, new string[] { });

				// Allow analysis to complete.
				await Task.Delay(5000);

				// Ensure we got the expected error in single_type_error.
				Assert.Equal(1, errors.Where(e => e.File.EndsWith("\\single_type_error.dart")).Distinct().Count());

				// Clear the error list ready for next time.
				errors.Clear();

				// Build a "fix" for this, which is to change the 1 to a string '1'.
				await service.UpdateContent(
					Path.Combine(SampleDartProject, "single_type_error.dart"),
					@"
						void main() {
							my_function('1');
						}

						void my_function(String a) {
						}
					"
				);

				// Allow analysis to complete.
				await Task.Delay(5000);

				// Ensure the error has gone away.
				Assert.Equal(0, errors.Where(e => e.File.EndsWith("\\single_type_error.dart")).Distinct().Count());
			}
		}
	}
}
