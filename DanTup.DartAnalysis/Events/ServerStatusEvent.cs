
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

	public struct ServerStatusNotification
	{
		public bool IsAnalysing { get; internal set; }
	}

	internal static class ServerStatusEventImplementation
	{
		public static ServerStatusNotification AsNotification(this ServerStatusEvent notification)
		{
			return new ServerStatusNotification { IsAnalysing = notification.analysis.analyzing };
		}
	}
}
