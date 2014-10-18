namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
	using System;

	public partial class DartGeneralPropertyPagePanel : DartPropertyPagePanel
	{
		public DartGeneralPropertyPagePanel()
			: this(null)
		{
		}

		public DartGeneralPropertyPagePanel(DartPropertyPage parentPropertyPage)
			: base(parentPropertyPage)
		{
			InitializeComponent();
		}

		public new DartGeneralPropertyPage ParentPropertyPage
		{
			get
			{
				return base.ParentPropertyPage as DartGeneralPropertyPage;
			}
		}

		public string ProjectFolder
		{
			get
			{
				return txtJavacPath.RootFolder;
			}

			set
			{
				txtJavacPath.RootFolder = value;
			}
		}

		public string JavacPath
		{
			get
			{
				return txtJavacPath.Text;
			}

			set
			{
				txtJavacPath.Text = value;
			}
		}

		// Javac Path
		private void folderBrowserTextBox1_TextChanged(object sender, EventArgs e)
		{
			ParentPropertyPage.IsDirty = true;
			if (ParentPropertyPage.ProjectManager != null && ParentPropertyPage.ProjectManager.SharedBuildOptions.Build != null)
				ParentPropertyPage.ProjectManager.SharedBuildOptions.Build.RefreshCommandLine();
		}
	}
}
