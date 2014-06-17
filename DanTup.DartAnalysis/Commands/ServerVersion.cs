namespace DanTup.DartAnalysis
{
	class VersionRequest : Request<Response<VersionResponse>>
	{
		public string method = "server.getVersion";
	}

	class VersionResponse
	{
		public string version;
	}
}
