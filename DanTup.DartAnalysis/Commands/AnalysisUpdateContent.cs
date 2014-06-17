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

	// TODO: Currently sending lenghs/offsets doesn't seem to do anything different to without, but
	// it sounds like partial updates may be supported in future, so we'll revisit this later.
	//class AnalysisUpdateContentFileWithOffset : AnalysisUpdateContentFile
	//{
	//	public int offset;
	//	public int oldLength;
	//	public int newLength;

	//	public AnalysisUpdateContentFileWithOffset(string content, int offset, int oldLength, int newLength)
	//		: base(content)
	//	{
	//		this.offset = offset;
	//		this.oldLength = oldLength;
	//		this.newLength = newLength;
	//	}
	//}

	public static class AnalysisUpdateContentImplementation
	{
		public static Task UpdateContent(this DartAnalysisService service, string filename, string contents)
		{
			return service.UpdateContent(new Dictionary<string, string> { { filename, contents } });
		}

		public static async Task UpdateContent(this DartAnalysisService service, Dictionary<string, string> files)
		{
			var response = await service.Service.Send(new AnalysisUpdateContentRequest(files.ToDictionary(kvp => kvp.Key, kvp => new AnalysisUpdateContentFile(kvp.Value))));

			// There's nothing useful on this response to return.

			return;
		}
	}
}
