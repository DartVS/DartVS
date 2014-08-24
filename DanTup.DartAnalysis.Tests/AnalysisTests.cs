using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
using Xunit;

namespace DanTup.DartAnalysis.Tests
{
	public class AnalysisTests : Tests
	{
		[Fact]
		public async Task SetAnalysisRoots()
		{
			using (var service = CreateTestService())
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();

				var errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (analysisCompleteEvent.Connect())
				{
					// Send a request to do some analysis.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Ensure the error-free file got no errors.
					Assert.Equal(0, errors.Where(e => e.Location.File == HelloWorldFile).Count());

					// Ensure the single-error file got the expected error.
					Assert.Equal(1, errors.Where(e => e.Location.File == SingleTypeErrorFile).Distinct().Count());
					var error = errors.First(e => e.Location.File == SingleTypeErrorFile);
					Assert.Equal(ErrorSeverity.Warning, error.Severity);
					Assert.Equal(ErrorType.StaticWarning, error.Type);
					Assert.Equal("The argument type 'int' cannot be assigned to the parameter type 'String'", error.Message);
				}
			}
		}

		[Fact]
		public async Task TestAnalysisUpdateContent()
		{
			using (var service = CreateTestService())
			{
				var firstAnalysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();
				var secondAnalysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();

				var errors = new List<AnalysisError>(); // Keep track of errors that are reported
				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (firstAnalysisCompleteEvent.Connect())
				{
					// Set the roots to our known project and wait for analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await firstAnalysisCompleteEvent;
				}

				// Ensure we got the expected error in single_type_error.
				Assert.Equal(1, errors.Where(e => e.Location.File == SingleTypeErrorFile).Distinct().Count());

				// Clear the error list ready for next time.
				errors.Clear();

				await Task.Delay(10000); // HACK: Allow the buffer to clear; since we're using ReplaySubjects now :/

				using (service.AnalysisErrorsNotification.Subscribe(e => errors.AddRange(e.Errors)))
				using (secondAnalysisCompleteEvent.Connect())
				{
					// Build a "fix" for this, which is to change the 1 to a string '1'.
					await service.UpdateContent(
						SingleTypeErrorFile,
						new AddContentOverlay
						{
							Type = "add",
							Content = @"
void main() {
	my_function('1');
}

void my_function(String a) {
}
"
						}
					);

					// Wait for a server status message (which should be that the analysis complete)
					await secondAnalysisCompleteEvent;
				}

				// Ensure the error has gone away.
				Assert.Equal(0, errors.Where(e => e.Location.File == SingleTypeErrorFile).Distinct().Count());
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsHighlights()
		{
			using (var service = CreateTestService())
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();
				var analysisHighlightEvent = service.AnalysisHighlightsNotification.FirstAsync().PublishLast();

				var regions = new List<HighlightRegion>(); // Keep track of errors that are reported
				using (service.AnalysisHighlightsNotification.Subscribe(e => regions.AddRange(e.Regions)))
				using (analysisCompleteEvent.Connect())
				using (analysisHighlightEvent.Connect())
				{
					// Set the roots to our known project and wait for the analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request Highlights and wait for it to complete (note: assuming first event back means it's complete).
					await service.SetAnalysisSubscriptions(new[] { AnalysisService.Highlights }, HelloWorldFile);
					await analysisHighlightEvent;

					// Ensure it's what we expect
					Assert.Equal(4, regions.Count);
					Assert.Equal(0, regions[0].Offset);
					Assert.Equal(4, regions[0].Length);
					Assert.Equal(HighlightRegionType.Keyword, regions[0].Type);
					Assert.Equal(5, regions[1].Offset);
					Assert.Equal(4, regions[1].Length);
					Assert.Equal(HighlightRegionType.FunctionDeclaration, regions[1].Type);
					Assert.Equal(17, regions[2].Offset);
					Assert.Equal(5, regions[2].Length);
					Assert.Equal(HighlightRegionType.Function, regions[2].Type);
					Assert.Equal(23, regions[3].Offset);
					Assert.Equal(15, regions[3].Length);
					Assert.Equal(HighlightRegionType.LiteralString, regions[3].Type);
				}
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsNavigation()
		{
			using (var service = CreateTestService())
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();
				var analysisNavigationEvent = service.AnalysisNavigationNotification.FirstAsync().PublishLast();

				var regions = new List<NavigationRegion>(); // Keep track of errors that are reported
				using (service.AnalysisNavigationNotification.Subscribe(e => regions.AddRange(e.Regions)))
				using (analysisCompleteEvent.Connect())
				using (analysisNavigationEvent.Connect())
				{
					// Set the roots to our known project and wait for the analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request Highlights and wait for it to complete (note: assuming first event back means it's complete).
					await service.SetAnalysisSubscriptions(new[] { AnalysisService.Navigation }, HelloWorldFile);
					await analysisNavigationEvent;

					// Ensure it's what we expect
					Assert.Equal(2, regions.Count);

					Assert.Equal(5, regions[0].Offset);
					Assert.Equal(4, regions[0].Length);
					Assert.Equal(1, regions[0].Targets.Length);
					Assert.Equal(HelloWorldFile, regions[0].Targets[0].Location.File);
					Assert.Equal(5, regions[0].Targets[0].Location.Offset);
					Assert.Equal(4, regions[0].Targets[0].Location.Length);
					Assert.Equal(1, regions[0].Targets[0].Location.StartLine);
					Assert.Equal(6, regions[0].Targets[0].Location.StartColumn);
					Assert.Equal(ElementKind.Function, regions[0].Targets[0].Kind);
					Assert.Equal("main", regions[0].Targets[0].Name);
					Assert.Equal(AnalysisElementFlags.Static, (AnalysisElementFlags)regions[0].Targets[0].Flags);
					Assert.Equal("()", regions[0].Targets[0].Parameters);
					Assert.Equal("void", regions[0].Targets[0].ReturnType);

					Assert.Equal(17, regions[1].Offset);
					Assert.Equal(5, regions[1].Length);
					Assert.Equal(1, regions[1].Targets.Length);
					Assert.Equal(Path.Combine(SdkFolder, @"lib\core\print.dart"), regions[1].Targets[0].Location.File, StringComparer.OrdinalIgnoreCase);
					Assert.Equal(307, regions[1].Targets[0].Location.Offset);
					Assert.Equal(5, regions[1].Targets[0].Location.Length);
					Assert.Equal(8, regions[1].Targets[0].Location.StartLine);
					Assert.Equal(6, regions[1].Targets[0].Location.StartColumn);
					Assert.Equal(ElementKind.Function, regions[1].Targets[0].Kind);
					Assert.Equal("print", regions[1].Targets[0].Name);
					Assert.Equal(AnalysisElementFlags.Static, (AnalysisElementFlags)regions[1].Targets[0].Flags);
					Assert.Equal("(Object object)", regions[1].Targets[0].Parameters);
					Assert.Equal("void", regions[1].Targets[0].ReturnType);
				}
			}
		}

		[Fact]
		public async Task AnalysisSetSubscriptionsOutline()
		{
			using (var service = CreateTestService())
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();
				var analysisOutlineEvent = service.AnalysisOutlineNotification.FirstAsync().PublishLast();

				var outlines = new List<Outline>(); // Keep track of errors that are reported
				using (service.AnalysisOutlineNotification.Subscribe(e => outlines.Add(e.Outline)))
				using (analysisCompleteEvent.Connect())
				using (analysisOutlineEvent.Connect())
				{
					// Set the roots to our known project.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request all the other stuff
					await service.SetAnalysisSubscriptions(new[] { AnalysisService.Outline }, HelloWorldFile);
					await analysisOutlineEvent;

					// Ensure it's what we expect
					var expectedOutline = new Outline
					{
						Element = new Element
						{
							Kind = ElementKind.CompilationUnit,
							Name = "<unit>",
							Location = new Location
							{
								File = HelloWorldFile,
								Offset = 0,
								Length = 45,
								StartLine = 1,
								StartColumn = 1,
							},
							Flags = (int)AnalysisElementFlags.None, // TODO: Change this if/when Flags is properly typed.
						},
						Offset = 0,
						Length = 45,
						Children = new[] {
							new Outline
							{
								Element = new Element
								{
									Kind = ElementKind.Function,
									Name = "main",
									Location = new Location
									{
										File = HelloWorldFile,
										Offset = 5,
										Length = 4,
										StartLine = 1,
										StartColumn = 6,
									},
									Flags = (int)AnalysisElementFlags.Static, // TODO: Change this if/when Flags is properly typed.
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

		[Fact]
		public async Task AnalysisGetHover()
		{
			using (var service = CreateTestService())
			{
				var analysisCompleteEvent = service.ServerStatusNotification.FirstAsync(n => n.Analysis.Analyzing == false).PublishLast();
				//var analysisHighlightEvent = service.AnalysisHighlightsNotification.FirstAsync().PublishLast();

				//var = new List<AnalysisHighlightRegion>(); // Keep track of errors that are reported
				//using (service.AnalysisHighlightsNotification.Subscribe(e => regions.AddRange(e.Regions)))
				using (analysisCompleteEvent.Connect())
				//using (analysisHighlightEvent.Connect())
				{
					// Set the roots to our known project and wait for the analysis to complete.
					await service.SetAnalysisRoots(new[] { SampleDartProject });
					await analysisCompleteEvent;

					// Request Highlights and wait for it to complete (note: assuming first event back means it's complete).
					var hovers = await service.GetHover(HelloWorldFile, 19);

					Assert.Equal(1, hovers.Length);
					Assert.Equal(17, hovers[0].Offset);
					Assert.Equal(5, hovers[0].Length);
					Assert.Equal(SdkFolder + "\\lib\\core\\core.dart", hovers[0].ContainingLibraryPath, StringComparer.OrdinalIgnoreCase);
					Assert.Equal("dart.core", hovers[0].ContainingLibraryName);
					Assert.Equal("Prints a string representation of the object to the console.", hovers[0].Dartdoc);
					Assert.Equal("function", hovers[0].ElementKind);
					Assert.Equal("print(Object object) → void", hovers[0].ElementDescription);
					Assert.Null(hovers[0].PropagatedType);
					Assert.Null(hovers[0].StaticType);
					Assert.Null(hovers[0].Parameter);
				}
			}
		}
	}
}
