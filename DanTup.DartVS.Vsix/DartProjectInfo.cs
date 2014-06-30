using EnvDTE;

namespace DanTup.DartVS
{
	public class DartProjectInfo
	{
		public string Path { get; private set; }
		public Project Project { get; private set; }

		public DartProjectInfo(string path, Project project)
		{
			this.Path = path;
			this.Project = project;
		}
	}
}
