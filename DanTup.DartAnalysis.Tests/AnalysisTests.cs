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
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();

				List<AnalysisError> errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (analysisCompleteEvent.Connect())
				{
					// Send a request to do some analysis.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

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
				var firstAnalysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();
				var secondAnalysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();

				List<AnalysisError> errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (firstAnalysisCompleteEvent.Connect())
				{
					// Set the roots to our known project and wait for analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await firstAnalysisCompleteEvent;
				}

				// Ensure we got the expected error in single_type_error.
				Assert.Equal(1, errors.Where(e => e.File == SingleTypeErrorFile).Distinct().Count());

				// Clear the error list ready for next time.
				errors.Clear();

				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (secondAnalysisCompleteEvent.Connect())
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
					await secondAnalysisCompleteEvent;
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
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();
				var analysisHighlightEvent = service.AnalysisHighlightsNotification.FirstAsync().PublishLast();

				List<AnalysisHighlightRegion> regions = new List<AnalysisHighlightRegion>(); // Keep track of errors that are reported
				using (service.AnalysisHighlightsNotification.Subscribe(e => regions.AddRange(e.Regions)))
				using (analysisCompleteEvent.Connect())
				using (analysisHighlightEvent.Connect())
				{
					// Set the roots to our known project and wait for the analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request Highlights and wait for it to complete (note: assuming first event back means it's complete).
					await service.SetAnalysisSubscriptions(new[] { AnalysisSubscription.Highlights }, HelloWorldFile);
					await analysisHighlightEvent;

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
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();
				var analysisNavigationEvent = service.AnalysisNavigationNotification.FirstAsync().PublishLast();

				List<AnalysisNavigationRegion> regions = new List<AnalysisNavigationRegion>(); // Keep track of errors that are reported
				using (service.AnalysisNavigationNotification.Subscribe(e => regions.AddRange(e.Regions)))
				using (analysisCompleteEvent.Connect())
				using (analysisNavigationEvent.Connect())
				{
					// Set the roots to our known project and wait for the analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request Highlights and wait for it to complete (note: assuming first event back means it's complete).
					await service.SetAnalysisSubscriptions(new[] { AnalysisSubscription.Navigation }, HelloWorldFile);
					await analysisNavigationEvent;

					// Ensure it's what we expect
					Assert.Equal(2, regions.Count);

					Assert.Equal(5, regions[0].Offset);
					Assert.Equal(4, regions[0].Length);
					Assert.Equal(1, regions[0].Targets.Length);
					Assert.Equal(HelloWorldFile, regions[0].Targets[0].File);
					Assert.Equal(5, regions[0].Targets[0].Offset);
					Assert.Equal(4, regions[0].Targets[0].Length);
					Assert.Equal(ElementKind.Function, regions[0].Targets[0].Element.Kind);
					Assert.Equal("main", regions[0].Targets[0].Element.Name);
					Assert.Equal(5, regions[0].Targets[0].Element.Offset);
					Assert.Equal(4, regions[0].Targets[0].Element.Length);
					Assert.Equal(AnalysisElementFlags.Static, regions[0].Targets[0].Element.Flags);
					Assert.Equal("()", regions[0].Targets[0].Element.Parameters);
					Assert.Equal("void", regions[0].Targets[0].Element.ReturnType);
					Assert.Null(regions[0].Targets[0].Element.Children);

					Assert.Equal(17, regions[1].Offset);
					Assert.Equal(5, regions[1].Length);
					Assert.Equal(1, regions[1].Targets.Length);
					Assert.Equal(Path.Combine(SdkFolder, @"lib\core\print.dart"), regions[1].Targets[0].File);
					Assert.Equal(307, regions[1].Targets[0].Offset);
					Assert.Equal(5, regions[1].Targets[0].Length);
					Assert.Equal(ElementKind.Function, regions[1].Targets[0].Element.Kind);
					Assert.Equal("print", regions[1].Targets[0].Element.Name);
					Assert.Equal(307, regions[1].Targets[0].Element.Offset);
					Assert.Equal(5, regions[1].Targets[0].Element.Length);
					Assert.Equal(AnalysisElementFlags.Static, regions[1].Targets[0].Element.Flags);
					Assert.Equal("(Object object)", regions[1].Targets[0].Element.Parameters);
					Assert.Equal("void", regions[1].Targets[0].Element.ReturnType);
					Assert.Null(regions[1].Targets[0].Element.Children);
				}
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsOutline()
		{
			using (var service = new DartAnalysisService(SdkFolder, ServerScript))
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.IsAnalysing == false).PublishLast();
				var analysisOutlineEvent = service.AnalysisOutlineNotification.FirstAsync().PublishLast();

				List<AnalysisOutline> outlines = new List<AnalysisOutline>(); // Keep track of errors that are reported
				using (service.AnalysisOutlineNotification.Subscribe(e => outlines.Add(e.Outline)))
				using (analysisCompleteEvent.Connect())
				using (analysisOutlineEvent.Connect())
				{
					// Set the roots to our known project.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request all the other stuff
					await service.SetAnalysisSubscriptions(new[] { AnalysisSubscription.Outline }, HelloWorldFile);
					await analysisOutlineEvent;

					// Ensure it's what we expect
					var expectedOutline = new AnalysisOutline
					{
						Element = new AnalysisElement
						{
							Kind = ElementKind.CompilationUnit,
							Name = "<unit>",
							Offset = 0,
							Length = 43,
							Flags = AnalysisElementFlags.None,
						},
						Offset = 0,
						Length = 43,
						Children = new[] {
							new AnalysisOutline
							{
								Element = new AnalysisElement
								{
									Kind = ElementKind.Function,
									Name = "main",
									Offset = 5,
									Length = 4,
									Flags = AnalysisElementFlags.Static,
									Parameters = "()",
									ReturnType = "void"
								},
								Offset = 0,
								Length = 43,
							}
						}
					};

					Assert.Equal(1, outlines.Count);
					Assert.Equal(expectedOutline.Children[0], outlines[0].Children[0]);
					// HACK: Arrays are references, and not considered equal. xUnit doesn't appear to have a way to assert
					// two structs are equal if they contain an array. Since we've already checked children, just fudge it
					// for now... :-/
					expectedOutline.Children = outlines[0].Children;
					Assert.Equal(expectedOutline, outlines[0]);
				}
			}
		}
	}
}
