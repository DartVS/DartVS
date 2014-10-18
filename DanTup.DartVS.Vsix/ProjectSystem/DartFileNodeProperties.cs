namespace DanTup.DartVS.ProjectSystem
{
	using System.ComponentModel;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;
	using prjBuildAction = VSLangProj.prjBuildAction;

	[ComVisible(true)]
	public class DartFileNodeProperties : FileNodeProperties
	{
		public DartFileNodeProperties(DartFileNode node)
			: base(node)
		{
		}

		[Browsable(false)]
		public override prjBuildAction BuildAction
		{
			get
			{
				return base.BuildAction;
			}

			set
			{
				base.BuildAction = value;
			}
		}

		[Browsable(false)]
		public override CopyToOutputDirectoryBehavior CopyToOutputDirectory
		{
			get
			{
				return base.CopyToOutputDirectory;
			}

			set
			{
				base.CopyToOutputDirectory = value;
			}
		}
	}
}
