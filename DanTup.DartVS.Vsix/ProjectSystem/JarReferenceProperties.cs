namespace DanTup.DartVS.ProjectSystem
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;

    [ComVisible(true)]
    public class JarReferenceProperties : ReferenceNodeProperties
    {
        public JarReferenceProperties(JarReferenceNode node)
            : base(node)
        {
        }
    }
}
