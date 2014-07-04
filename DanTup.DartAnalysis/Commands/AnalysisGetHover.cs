using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	class AnalysisGetHoverRequest : Request<AnalysisGetHoverParams, Response<AnalysisGetHoverResponse>>
	{
		public string method = "analysis.getHover";

		public AnalysisGetHoverRequest(string file, int offset)
		{
			this.@params = new AnalysisGetHoverParams(file, offset);
		}
	}

	class AnalysisGetHoverParams
	{
		public string file;
		public int offset;

		public AnalysisGetHoverParams(string file, int offset)
		{
			this.file = file;
			this.offset = offset;
		}
	}

	class AnalysisGetHoverResponse
	{
		public AnalysisHoverItem[] hovers = null;
	}

	public class AnalysisHoverItem
	{
		public int offset;
		public int length;
		public string containingLibraryPath;
		public string containingLibraryName;
		public string dartdoc;
		public string elementKind;
		public string elementDescription;
		public string propagatedType;
		public string staticType;
		public string parameter;
	}

	public static class AnalysisGetHoverImplementation
	{
		public static async Task<AnalysisHoverItem[]> GetHover(this DartAnalysisService service, string file, int offset)
		{
			var response = await service.Service.Send(new AnalysisGetHoverRequest(file, offset));

			return response.result.hovers;
		}
	}
}
