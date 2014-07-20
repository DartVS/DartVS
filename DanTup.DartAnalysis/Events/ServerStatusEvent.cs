
namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class ServerStatusEventJson
	{
		public ServerAnalysisStatusJson analysis = null;
	}

	class ServerAnalysisStatusJson
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
		public static ServerStatusNotification AsNotification(this ServerStatusEventJson notification)
		{
			return new ServerStatusNotification { IsAnalysing = notification.analysis.analyzing };
		}
	}
}
