using System;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class ServerStatusEvent
	{
		public ServerAnalysisStatus analysis = null;
	}

	class ServerAnalysisStatus
	{
		public bool analyzing = false;
	}

	#endregion

	public class ServerStatusNotification
	{
		public bool IsAnalysing { get; internal set; }
	}

	internal static class ServerStatusEventImplementation
	{
		public static void RaiseServerStatusEvent(this DartAnalysisService service, ServerStatusEvent notification, EventHandler<ServerStatusNotification> handler)
		{
			if (handler != null)
				handler.Invoke(service, new ServerStatusNotification { IsAnalysing = notification.analysis.analyzing });
		}
	}
}
