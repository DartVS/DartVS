namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
	using System;
	using System.Runtime.InteropServices;
	using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

	[ComVisible(true)]
	[Guid(DartProjectConstants.GeneralPropertyPageGuidString)]
	public class DartGeneralPropertyPage : DartPropertyPage
	{
		public DartGeneralPropertyPage()
		{
			PageName = DartConfigConstants.PageNameGeneral;
		}

		public new DartGeneralPropertyPagePanel PropertyPagePanel
		{
			get
			{
				return (DartGeneralPropertyPagePanel)base.PropertyPagePanel;
			}
		}

		protected override void BindProperties()
		{
			if (ProjectManager != null)
				ProjectManager.SharedBuildOptions.General = PropertyPagePanel;

			PropertyPagePanel.ProjectFolder = ProjectManager.ProjectFolder;
			PropertyPagePanel.JavacPath = GetConfigProperty(DartConfigConstants.JavacPath, _PersistStorageType.PST_PROJECT_FILE);
		}

		protected override bool ApplyChanges()
		{
			SetProperty(DartConfigConstants.JavacPath, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.JavacPath);
			return true;
		}

		protected override DartPropertyPagePanel CreatePropertyPagePanel()
		{
			return new DartGeneralPropertyPagePanel(this);
		}
	}
}
