namespace DanTup.DartVS.ProjectSystem
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;

    [ComVisible(true)]
    public class DartFolderNode : FolderNode
    {
        public DartFolderNode(ProjectNode root, string relativePath, ProjectElement element)
            : base(root, relativePath, element)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (relativePath == null)
                throw new ArgumentNullException("relativePath");
            if (element == null)
                throw new ArgumentNullException("element");

            if (element.IsVirtual)
            {
                string buildAction = element.GetMetadata(ProjectFileConstants.BuildAction);
                if (buildAction == ProjectFileConstants.Folder)
                    this.IsNonmemberItem = false;
            }
        }

        public new DartProjectNode ProjectManager
        {
            get
            {
                return (DartProjectNode)base.ProjectManager;
            }
        }

        public override object GetIconHandle(bool open)
        {
            if (this.IsNonmemberItem)
                return base.GetIconHandle(open);

            if (string.Equals(ItemNode.ItemName, DartProjectFileConstants.SourceFolder))
                return this.ProjectManager.ExtendedImageHandler.GetIconHandle(open ? (int)DartProjectNode.ExtendedImageName.OpenSourceFolder : (int)DartProjectNode.ExtendedImageName.SourceFolder);

            if (string.Equals(ItemNode.ItemName, DartProjectFileConstants.TestSourceFolder))
                return this.ProjectManager.ExtendedImageHandler.GetIconHandle(open ? (int)DartProjectNode.ExtendedImageName.OpenTestSourceFolder: (int)DartProjectNode.ExtendedImageName.TestSourceFolder);

            return base.GetIconHandle(open);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new DartFolderNodeProperties(this);
        }
    }
}
