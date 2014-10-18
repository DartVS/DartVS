namespace DanTup.DartVS.ProjectSystem
{
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;

	[ComVisible(true)]
	public class DartFolderNodeProperties : FolderNodeProperties
	{
		public DartFolderNodeProperties(DartFolderNode node)
			: base(node)
		{
		}
	}
}
