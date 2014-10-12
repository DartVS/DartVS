namespace DanTup.DartVS.ProjectSystem
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;

    using IVsComponentSelectorProvider = Microsoft.VisualStudio.Shell.Interop.IVsComponentSelectorProvider;
    using VSConstants = Microsoft.VisualStudio.VSConstants;
    using VSPROPSHEETPAGE = Microsoft.VisualStudio.Shell.Interop.VSPROPSHEETPAGE;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(DartProjectConstants.ProjectPackageNameResourceString, DartProjectConstants.ProjectPackageDetailsResourceString, DartProjectConstants.ProjectPackageProductVersionString/*, IconResourceID = 400*/)]
    [Guid(DartProjectConstants.ProjectPackageGuidString)]
    [ProvideProjectFactory(
        typeof(DartProjectFactory),
        "Dart",
        "Dart Project Files (*.dartproj);*.dartproj",
        "dartproj",
        "dartproj",
        "ProjectTemplates",
        LanguageVsTemplate = "Dart",
        NewProjectRequireNewFolderVsTemplate = false)]

    [ProvideObject(typeof(PropertyPages.DartApplicationPropertyPage))]
    [ProvideObject(typeof(PropertyPages.DartBuildEventsPropertyPage))]
    [ProvideObject(typeof(PropertyPages.DartBuildPropertyPage))]
    [ProvideObject(typeof(PropertyPages.DartDebugPropertyPage))]
    [ProvideObject(typeof(PropertyPages.DartGeneralPropertyPage))]

    [ProvideComponentSelectorTab(typeof(PropertyPages.MavenComponentSelector), "Maven")]
    public class DartProjectPackage : ProjectPackage, IVsComponentSelectorProvider
    {
        private PropertyPages.MavenComponentSelector _mavenComponentSelector;

        public override string ProductUserContext
        {
            get
            {
                return "Dart";
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            RegisterProjectFactory(new DartProjectFactory(this));
        }

        #region IVsComponentSelectorProvider Members

        public int GetComponentSelectorPage(ref Guid rguidPage, VSPROPSHEETPAGE[] ppage)
        {
            if (ppage == null)
                throw new ArgumentNullException("ppage");
            if (ppage.Length == 0)
                throw new ArgumentException();

            if (rguidPage == DartProjectConstants.MavenComponentSelectorGuid)
            {
                _mavenComponentSelector = _mavenComponentSelector ?? new PropertyPages.MavenComponentSelector();

                ppage[0] = new VSPROPSHEETPAGE()
                    {
                        dwFlags = (uint)default(PropertySheetPageFlags),
                        dwReserved = 0,
                        dwSize = (uint)Marshal.SizeOf(typeof(VSPROPSHEETPAGE)),
                        dwTemplateSize = 0,
                        HINSTANCE = 0,
                        hwndDlg = _mavenComponentSelector.Handle,
                        lParam = IntPtr.Zero,
                        pcRefParent = IntPtr.Zero,
                        pfnCallback = IntPtr.Zero,
                        pfnDlgProc = IntPtr.Zero,
                        pTemplate = IntPtr.Zero,
                        wTemplateId = 0,
                    };

                return VSConstants.S_OK;
            }

            return VSConstants.E_INVALIDARG;
        }

        #endregion
    }
}
