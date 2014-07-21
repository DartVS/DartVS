using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	class AnalysisUpdateContentRequest : Request<AnalysisUpdateContentParams, Response>
	{
		public string method = "analysis.updateContent";

		public AnalysisUpdateContentRequest(Dictionary<string, AnalysisUpdateContentFile> files)
		{
			this.@params = new AnalysisUpdateContentParams(files);
		}
	}

	class AnalysisUpdateContentParams
	{
		public Dictionary<string, AnalysisUpdateContentFile> files;

		public AnalysisUpdateContentParams(Dictionary<string, AnalysisUpdateContentFile> files)
		{
			this.files = files;
		}
	}

	class AnalysisUpdateContentFile
	{
		public string content;

		public AnalysisUpdateContentFile(string content)
		{
			this.content = content;
		}
	}

	public static class AnalysisUpdateContentImplementation
	{
		public static Task UpdateContent(this DartAnalysisService service, string filename, string contents)
		{
			return service.UpdateContent(new Dictionary<string, string> { { filename, contents } });
		}

		public static async Task UpdateContent(this DartAnalysisService service, Dictionary<string, string> files)
		{
			var response = await service.Service.Send(new AnalysisUpdateContentRequest(files.ToDictionary(kvp => kvp.Key, kvp => new AnalysisUpdateContentFile(kvp.Value)))).ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
