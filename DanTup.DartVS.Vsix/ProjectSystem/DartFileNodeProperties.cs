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

        public override CopyToOutputDirectoryBehavior CopyToOutputDirectory
        {
            get
            {
                return base.CopyToOutputDirectory;
            }

            set
            {
                if (Node.ItemNode.IsVirtual && value != CopyToOutputDirectoryBehavior.DoNotCopy)
                {
                    Node.ItemNode = Node.ProjectManager.AddFileToMSBuild(Node.VirtualNodeName, ProjectFileConstants.Content, null);
                }

                base.CopyToOutputDirectory = value;
            }
        }
    }
}
