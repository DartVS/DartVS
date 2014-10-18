namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
	using System;
	using System.Runtime.InteropServices;
	using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

	[ComVisible(true)]
	[Guid(DartProjectConstants.BuildPropertyPageGuidString)]
	public class DartBuildPropertyPage : DartPropertyPage
	{
		public DartBuildPropertyPage()
		{
			PageName = DartConfigConstants.PageNameBuild;
		}

		public new DartBuildPropertyPagePanel PropertyPagePanel
		{
			get
			{
				return (DartBuildPropertyPagePanel)base.PropertyPagePanel;
			}
		}

		protected override void BindProperties()
		{
			if (ProjectManager != null)
				ProjectManager.SharedBuildOptions.Build = PropertyPagePanel;

			// general
			PropertyPagePanel.SourceRelease = GetConfigProperty(DartConfigConstants.SourceRelease, _PersistStorageType.PST_PROJECT_FILE);
			PropertyPagePanel.TargetRelease = GetConfigProperty(DartConfigConstants.TargetRelease, _PersistStorageType.PST_PROJECT_FILE);
			PropertyPagePanel.Encoding = GetConfigProperty(DartConfigConstants.SourceEncoding, _PersistStorageType.PST_PROJECT_FILE);

			// debugging
			DebuggingInformation info;
			if (!Enum.TryParse(GetConfigProperty(DartConfigConstants.DebugSymbols, _PersistStorageType.PST_PROJECT_FILE), out info))
				info = DebuggingInformation.Default;

			PropertyPagePanel.DebuggingInformation = info;
			PropertyPagePanel.SpecificDebuggingInformation = GetConfigProperty(DartConfigConstants.SpecificDebugSymbols, _PersistStorageType.PST_PROJECT_FILE);

			// warnings
			PropertyPagePanel.ShowWarnings = GetConfigPropertyBoolean(DartConfigConstants.ShowWarnings, _PersistStorageType.PST_PROJECT_FILE);
			PropertyPagePanel.ShowAllWarnings = GetConfigPropertyBoolean(DartConfigConstants.ShowAllWarnings, _PersistStorageType.PST_PROJECT_FILE);

			// warnings as errors
			WarningsAsErrors warnAsError;
			if (!Enum.TryParse(GetConfigProperty(DartConfigConstants.TreatWarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE), out warnAsError))
				warnAsError = WarningsAsErrors.None;

			PropertyPagePanel.WarningsAsErrors = warnAsError;
			PropertyPagePanel.SpecificWarningsAsErrors = GetConfigProperty(DartConfigConstants.WarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE);

			// output
			PropertyPagePanel.OutputPath = GetConfigProperty(DartConfigConstants.OutputPath, _PersistStorageType.PST_PROJECT_FILE);

			// extra arguments
			PropertyPagePanel.ExtraArguments = GetConfigProperty(DartConfigConstants.BuildArgs, _PersistStorageType.PST_PROJECT_FILE);
		}

		protected override bool ApplyChanges()
		{
			// general
			SetConfigProperty(DartConfigConstants.SourceRelease, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SourceRelease);
			SetConfigProperty(DartConfigConstants.TargetRelease, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.TargetRelease);
			SetConfigProperty(DartConfigConstants.SourceEncoding, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.Encoding);

			// debugging
			SetConfigProperty(DartConfigConstants.DebugSymbols, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.DebuggingInformation.ToString());
			SetConfigProperty(DartConfigConstants.SpecificDebugSymbols, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SpecificDebuggingInformation);

			// warnings
			SetConfigProperty(DartConfigConstants.ShowWarnings, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ShowWarnings);
			SetConfigProperty(DartConfigConstants.ShowAllWarnings, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ShowAllWarnings);

			// warnings as errors
			SetConfigProperty(DartConfigConstants.TreatWarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.WarningsAsErrors.ToString());
			SetConfigProperty(DartConfigConstants.WarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SpecificWarningsAsErrors);

			// output
			SetConfigProperty(DartConfigConstants.OutputPath, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.OutputPath);

			// extra arguments
			SetConfigProperty(DartConfigConstants.BuildArgs, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ExtraArguments);

			return true;
		}

		protected override DartPropertyPagePanel CreatePropertyPagePanel()
		{
			return new DartBuildPropertyPagePanel(this);
		}
	}
}
