using System;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;
namespace DanTup.DartAnalysis.Tests
{
	public abstract class Tests
	{
		protected string SdkFolder
		{
			get
			{
				string result = Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Process);
				if (!File.Exists(Path.Combine(result, "bin", "dart.exe")))
					throw new NotSupportedException("Could not locate or download the Dart SDK. All analysis is disabled.");

				return result;
			}
		}

		string CodebaseRoot = Path.GetFullPath(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), @"..\..\..\")).AbsolutePath); // up out of debug, bin, Tests

		protected string ServerScript { get { return Path.Combine(CodebaseRoot, "Dart\\analysis_server.dart.snapshot"); } }
		protected string SampleDartProject { get { return Path.Combine(CodebaseRoot, "DanTup.DartAnalysis.Tests.SampleDartProject"); } }
		protected string HelloWorldFile { get { return SampleDartProject + @"\hello_world.dart"; } }
		protected string SingleTypeErrorFile { get { return SampleDartProject + @"\single_type_error.dart"; } }

		protected DartAnalysisService CreateTestService()
		{
			var service = new DartAnalysisService(SdkFolder, ServerScript);

			service.SetServerSubscriptions(new[] { ServerService.Status }).Wait();

			return service;
		}
	}

	public static class TestExtensions
	{
		public static async Task WaitForAnalysis(this DartAnalysisService service)
		{
			await service
				.ServerStatusNotification
				.FirstAsync(n => n.Analysis.IsAnalyzing == false);
		}
	}
}
