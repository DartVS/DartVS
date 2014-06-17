using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanTup.DartAnalysis.Commands
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
