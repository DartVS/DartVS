namespace DanTup.DartVS.ProjectSystem
{
    using Guid = System.Guid;

    internal static class DartProjectConstants
    {
        public const int ProjectResourceId = 200;
        public const string ProjectPackageNameResourceString = "#210";
        public const string ProjectPackageDetailsResourceString = "#211";
        public const string ProjectPackageProductVersionString = "1.0";

        public const string ProjectPackageGuidString = "2DB200C2-8B0C-499B-859C-D3DF3ACD0301";
        public static readonly Guid ProjectPackageGuid = new Guid("{" + ProjectPackageGuidString + "}");

        public const string ProjectFactoryGuidString = "C7B71213-EAD8-427C-B825-175306E51BE9";
        public static readonly Guid ProjectGuid = new Guid("{" + ProjectFactoryGuidString + "}");

        // Property pages
        public const string GeneralPropertyPageGuidString = "FB758EB9-A364-449D-B819-9A6DA3E7283A";
        public static readonly Guid GeneralPropertyPageGuid = new Guid("{" + GeneralPropertyPageGuidString + "}");

        public const string ApplicationPropertyPageGuidString = "2807A03B-780B-444C-A7EF-300892F2DACA";
        public static readonly Guid ApplicationPropertyPageGuid = new Guid("{" + ApplicationPropertyPageGuidString + "}");

        public const string BuildEventsPropertyPageGuidString = "33D98078-39AA-41F2-9114-23E2479A2842";
        public static readonly Guid BuildEventsPropertyPageGuid = new Guid("{" + BuildEventsPropertyPageGuidString + "}");

        public const string BuildPropertyPageGuidString = "153F8CE0-C60A-41AD-AF25-CFD0D41114D6";
        public static readonly Guid BuildPropertyPageGuid = new Guid("{" + BuildPropertyPageGuidString + "}");

        public const string DebugPropertyPageGuidString = "056B4C57-3CA6-44F7-9365-51C104C3627B";
        public static readonly Guid DebugPropertyPageGuid = new Guid("{" + DebugPropertyPageGuidString + "}");

        // Component selector pages
        public const string MavenComponentSelectorGuidString = "16CDA776-DB23-42F9-BFB4-FA8308519CB3";
        public static readonly Guid MavenComponentSelectorGuid = new Guid("{" + MavenComponentSelectorGuidString + "}");
    }
}
