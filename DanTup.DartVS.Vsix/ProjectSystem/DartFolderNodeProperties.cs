namespace DanTup.DartVS.ProjectSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;

    [ComVisible(true)]
    public class DartFolderNodeProperties : FolderNodeProperties
    {
        public DartFolderNodeProperties(DartFolderNode node)
            : base(node)
        {
        }

        [Category("Advanced")]
        [DisplayName("Build Action")]
        [DefaultValue(FolderBuildAction.Folder)]
        public virtual FolderBuildAction BuildAction
        {
            get
            {
                string value = this.Node.ItemNode.ItemName;
                if (string.IsNullOrEmpty(value))
                    return FolderBuildAction.Folder;

                FolderBuildAction result;
                if (!Enum.TryParse(value, out result))
                    result = FolderBuildAction.Folder;

                return result;
            }

            set
            {
                DartProjectNode projectNode = (DartProjectNode)this.Node.ProjectManager;
                projectNode.UpdateFolderBuildAction((DartFolderNode)this.Node, value);
            }
        }
    }
}
