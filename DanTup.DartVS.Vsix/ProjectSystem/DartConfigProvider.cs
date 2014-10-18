namespace DanTup.DartVS.ProjectSystem
{
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.Project;

	using MSBuild = Microsoft.Build.Evaluation;

	public class DartConfigProvider : ConfigProvider
	{
		public const string DisplayAnyCPU = "Any CPU";
		public const string DisplayX86 = "X86";
		public const string DisplayX64 = "X64";

		public DartConfigProvider(DartProjectNode manager)
			: base(manager)
		{
		}

		protected new DartProjectNode ProjectManager
		{
			get
			{
				return (DartProjectNode)base.ProjectManager;
			}
		}

		protected override ProjectConfig CreateProjectConfiguration(string configName, string platform)
		{
			return new DartProjectConfig(this.ProjectManager, configName, platform);
		}

		public override string GetPlatformNameFromPlatformProperty(string platformProperty)
		{
			switch (platformProperty)
			{
			case DartProjectFileConstants.AnyCPU:
				return DisplayAnyCPU;

			case DartProjectFileConstants.X86:
				return DisplayX86;

			case DartProjectFileConstants.X64:
				return DisplayX64;

			default:
				return base.GetPlatformNameFromPlatformProperty(platformProperty);
			}
		}

		public override string GetPlatformPropertyFromPlatformName(string platformName)
		{
			switch (platformName)
			{
			case DisplayAnyCPU:
				return DartProjectFileConstants.AnyCPU;

			case DisplayX86:
				return DartProjectFileConstants.X86;

			case DisplayX64:
				return DartProjectFileConstants.X64;

			default:
				return base.GetPlatformPropertyFromPlatformName(platformName);
			}
		}

		protected override IEnumerable<MSBuild.Project> GetBuildProjects(bool includeUserBuildProjects = true)
		{
			if (!includeUserBuildProjects || ProjectManager.UserBuildProject == null)
				return base.GetBuildProjects(includeUserBuildProjects);

			return base.GetBuildProjects(false).Concat(new[] { ProjectManager.UserBuildProject });
		}
	}
}
