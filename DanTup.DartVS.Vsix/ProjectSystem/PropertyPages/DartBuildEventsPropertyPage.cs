namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
    using System;
    using System.Runtime.InteropServices;
    using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

    [ComVisible(true)]
    [Guid(DartProjectConstants.BuildEventsPropertyPageGuidString)]
    public class DartBuildEventsPropertyPage : DartPropertyPage
    {
        public DartBuildEventsPropertyPage()
        {
            PageName = DartConfigConstants.PageNameBuildEvents;
        }

        public new DartBuildEventsPropertyPagePanel PropertyPagePanel
        {
            get
            {
                return (DartBuildEventsPropertyPagePanel)base.PropertyPagePanel;
            }
        }

        protected override void BindProperties()
        {
            PropertyPagePanel.PreBuildEvent = GetConfigProperty(DartConfigConstants.PreBuildEvent, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.PostBuildEvent = GetConfigProperty(DartConfigConstants.PostBuildEvent, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.RunPostBuildEvent = GetConfigProperty(DartConfigConstants.RunPostBuildEvent, _PersistStorageType.PST_PROJECT_FILE);
        }
        protected override bool ApplyChanges()
        {
            SetConfigProperty(DartConfigConstants.PreBuildEvent, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.PreBuildEvent);
            SetConfigProperty(DartConfigConstants.PostBuildEvent, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.PostBuildEvent);
            SetConfigProperty(DartConfigConstants.RunPostBuildEvent, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.RunPostBuildEvent);
            return true;
        }

        protected override DartPropertyPagePanel CreatePropertyPagePanel()
        {
            return new DartBuildEventsPropertyPagePanel(this);
        }
    }
}
