using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public static class AnalysisUpdateContentImplementation
	{
		public static Task UpdateContent(this DartAnalysisService service, string filename, IAddContentOverlayOrChangeContentOverlayOrRemoveContentOverlay contents)
		{
			return service.UpdateContent(new Dictionary<string, IAddContentOverlayOrChangeContentOverlayOrRemoveContentOverlay> { { filename, contents } });
		}

		public static async Task UpdateContent(this DartAnalysisService service, Dictionary<string, IAddContentOverlayOrChangeContentOverlayOrRemoveContentOverlay> files)
		{
			var request = new AnalysisUpdateContentRequest {
				Files = files
			};

			var response = await service.Service
				.Send(new AnalysisUpdateContent(request))
				.ConfigureAwait(continueOnCapturedContext: false);

			// There's nothing useful on this response to return.

			return;
		}
	}
}
