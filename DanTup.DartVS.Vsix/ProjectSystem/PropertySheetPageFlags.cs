namespace DanTup.DartVS.ProjectSystem
{
    using System;
    using VSPROPSHEETPAGE = Microsoft.VisualStudio.Shell.Interop.VSPROPSHEETPAGE;

    /// <summary>
    /// Defines the valid flags for <see cref="VSPROPSHEETPAGE.dwFlags"/>.
    /// </summary>
    [Flags]
    public enum PropertySheetPageFlags : uint
    {
        /// <summary>
        /// Uses the default meaning for all structure members.
        /// </summary>
        PSP_DEFAULT = 0x0000,

        /// <summary>
        /// Creates the page from the dialog box template in memory pointed to by the
        /// <see cref="VSPROPSHEETPAGE.pTemplate"/> member. The <strong>PropertySheet</strong> function assumes that the
        /// template that is in memory is not write-protected. A read-only template will cause an exception in some
        /// versions of Microsoft Windows.
        /// </summary>
        PSP_DLGINDIRECT = 0x0001,

        /// <summary>
        /// Enables the property sheet <strong>Help</strong> button when the page is active.
        /// </summary>
        PSP_HASHELP = 0x0020,

        /// <summary>
        /// Maintains the reference count specified by the <see cref="VSPROPSHEETPAGE.pcRefParent"/> member for the
        /// lifetime of the property sheet page created from this structure.
        /// </summary>
        PSP_USEREFPARENT = 0x0040,

        /// <summary>
        /// Calls the function specified by the <see cref="VSPROPSHEETPAGE.pfnCallback"/> member when creating or
        /// destroying the property sheet page defined by this structure.
        /// </summary>
        PSP_USECALLBACK = 0x0080,

        /// <summary>
        /// Causes the page to be created when the property sheet is created. If this flag is not specified, the page
        /// will not be created until it is selected the first time.
        /// </summary>
        PSP_PREMATURE = 0x0400,

        /// <summary>
        /// Causes the wizard property sheet to hide the header area when the page is selected. If a watermark has been
        /// provided, it will be painted on the left side of the page. This flag should be set for welcome and
        /// completion pages, and omitted for interior pages.
        /// </summary>
        PSP_HIDEHEADER = 0x0800,
    }
}
