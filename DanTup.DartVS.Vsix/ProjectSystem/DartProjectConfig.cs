namespace DanTup.DartVS.ProjectSystem
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell.Interop;

    public class DartProjectConfig : ProjectConfig
    {
        private DartBuildableProjectConfig buildableCfg;

        internal DartProjectConfig(DartProjectNode project, string configuration, string platform)
            : base(project, configuration, platform)
        {
        }

        public new DartProjectNode ProjectManager
        {
            get
            {
                return (DartProjectNode)base.ProjectManager;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }

        public override int QueryDebugLaunch(uint flags, out int fCanLaunch)
        {
            fCanLaunch = 0;
            return VSConstants.S_OK;
        }

        public override int get_BuildableProjectCfg(out IVsBuildableProjectCfg pb)
        {
            if (buildableCfg == null)
                buildableCfg = new DartBuildableProjectConfig(this);

            pb = buildableCfg;
            return VSConstants.S_OK;
        }
    }
}
