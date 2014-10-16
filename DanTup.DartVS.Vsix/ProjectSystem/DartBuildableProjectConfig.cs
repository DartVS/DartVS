using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

namespace DanTup.DartVS.ProjectSystem
{
	internal class DartBuildableProjectConfig : BuildableProjectConfig
	{
		public DartBuildableProjectConfig(ProjectConfig config)
			: base(config)
		{
		}

		public override int QueryStartBuild(uint options, int[] supported, int[] ready)
		{
			if (supported != null && supported.Length > 0)
				supported[0] = 0;

			if (ready != null && ready.Length > 0)
				ready[0] = 0;

			return VSConstants.S_OK;
		}

		public override int QueryStartClean(uint options, int[] supported, int[] ready)
		{
			if (supported != null && supported.Length > 0)
				supported[0] = 0;

			if (ready != null && ready.Length > 0)
				ready[0] = 0;

			return VSConstants.S_OK;
		}
	}
}
