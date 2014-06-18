using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
				bool? isAnalyzing = null; // Keep track of when we're called to say server status has changed
				List<AnalysisError> errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.ServerStatusNotification.Subscribe(e => { isAnalyzing = e.IsAnalysing; }))
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				{
					// Send a request to do some analysis.
					await service.SetAnalysisRoots(new[] { SampleDartProject });

					// Wait for a server status message (which should be that the analysis complete)
					await service.ServerStatusNotification.FirstAsync();


					// Ensure the event fired to say analysing had finished.
					Assert.Equal(false, isAnalyzing);

					// Ensure the error-free file got no errors.
					Assert.Equal(0, errors.Where(e => e.File == HelloWorldFile).Count());

					// Ensure the single-error file got the expected error.
					Assert.Equal(1, errors.Where(e => e.File == SingleTypeErrorFile).Distinct().Count());
					var error = errors.First(e => e.File == SingleTypeErrorFile);
					Assert.Equal("StaticWarningCode.ARGUMENT_TYPE_NOT_ASSIGNABLE", error.ErrorCode);
				}
			}
		}

		[Fact]
		public async Task TestAnalysisUpdateContent()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				List<AnalysisError> errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				{
					// Set the roots to our known project.
					await service.SetAnalysisRoots(new[] { SampleDartProject });

					// Wait for a server status message (which should be that the analysis complete)
					await service.ServerStatusNotification.FirstAsync();
				}

				// Ensure we got the expected error in single_type_error.
				Assert.Equal(1, errors.Where(e => e.File == SingleTypeErrorFile).Distinct().Count());

				// Clear the error list ready for next time.
				errors.Clear();

				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				{

					// Build a "fix" for this, which is to change the 1 to a string '1'.
					await service.UpdateContent(
						SingleTypeErrorFile,
						@"
						void main() {
							my_function('1');
						}

						void my_function(String a) {
						}
					"
					);

					// Wait for a server status message (which should be that the analysis complete)
					await service.ServerStatusNotification.FirstAsync();
				}

				// Ensure the error has gone away.
				Assert.Equal(0, errors.Where(e => e.File == SingleTypeErrorFile).Distinct().Count());
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsHighlights()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				List<AnalysisHighlightRegion> regions = new List<AnalysisHighlightRegion>(); // Keep track of errors that are reported
				using (service.AnalysisHighlightsNotification.Subscribe(e => regions.AddRange(e.Regions)))
				{
					// Set the roots to our known project.
					await service.SetAnalysisRoots(new[] { SampleDartProject });

					// Request all the other stuff
					await service.SetAnalysisSubscriptions(new[] { "HIGHLIGHTS" }, HelloWorldFile);

					// Wait for a server status message (which should be that the analysis complete)
					await service.ServerStatusNotification.FirstAsync();

					// Ensure it's what we expect
					Assert.Equal(4, regions.Count);
					Assert.Equal(0, regions[0].Offset);
					Assert.Equal(4, regions[0].Length);
					Assert.Equal(HighlightType.Keyword, regions[0].Type);
					Assert.Equal(5, regions[1].Offset);
					Assert.Equal(4, regions[1].Length);
					Assert.Equal(HighlightType.FunctionDeclaration, regions[1].Type);
					Assert.Equal(17, regions[2].Offset);
					Assert.Equal(5, regions[2].Length);
					Assert.Equal(HighlightType.Function, regions[2].Type);
					Assert.Equal(23, regions[3].Offset);
					Assert.Equal(15, regions[3].Length);
					Assert.Equal(HighlightType.LiteralString, regions[3].Type);
				}
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsNavigation()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				List<AnalysisNavigationRegion> regions = new List<AnalysisNavigationRegion>(); // Keep track of errors that are reported
				using (service.AnalysisNavigationNotification.Subscribe(e => regions.AddRange(e.Regions)))
				{
					// Set the roots to our known project.
					await service.SetAnalysisRoots(new[] { SampleDartProject });

					// Request all the other stuff
					await service.SetAnalysisSubscriptions(new[] { "NAVIGATION" }, HelloWorldFile);

					// Wait for a server status message (which should be that the analysis complete)
					await service.ServerStatusNotification.FirstAsync();

					// Ensure it's what we expect
					Assert.Equal(2, regions.Count);

					Assert.Equal(5, regions[0].Offset);
					Assert.Equal(4, regions[0].Length);
					Assert.Equal(1, regions[0].Targets.Length);
					Assert.Equal(HelloWorldFile, regions[0].Targets[0].File);
					Assert.Equal(5, regions[0].Targets[0].Offset);
					Assert.Equal(4, regions[0].Targets[0].Length);
					Assert.Equal("ffile:///" + HelloWorldFile.Replace(@"\", "/") + ";ffile:///" + HelloWorldFile.Replace(@"\", "/") + ";main@5", regions[0].Targets[0].ElementID);

					Assert.Equal(17, regions[1].Offset);
					Assert.Equal(5, regions[1].Length);
					Assert.Equal(1, regions[1].Targets.Length);
					Assert.Equal(Path.Combine(SdkFolder, @"lib\core\print.dart"), regions[1].Targets[0].File);
					Assert.Equal(307, regions[1].Targets[0].Offset);
					Assert.Equal(5, regions[1].Targets[0].Length);
					Assert.Equal("dfile:///" + SdkFolder.Replace(@"\", "/") + "/lib/core/core.dart;dfile:///" + SdkFolder.Replace(@"\", "/") + "/lib/core/print.dart;print@307", regions[1].Targets[0].ElementID);
				}
			}
		}
	}
}
