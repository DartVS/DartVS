namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
	using System;
	using System.Collections.ObjectModel;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;
	using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

	[ComVisible(true)]
	[Guid(DartProjectConstants.ApplicationPropertyPageGuidString)]
	public class DartApplicationPropertyPage : DartPropertyPage
	{
		private static readonly string NotSetStartupObject = string.Empty;

		private static readonly ReadOnlyCollection<string> _defaultAvailableTargetVirtualMachines =
			new ReadOnlyCollection<string>(new string[]
			{
				DartProjectFileConstants.HotspotTargetVirtualMachine,
				DartProjectFileConstants.JRockitTargetVirtualMachine,
			});
		private static readonly ReadOnlyCollection<string> _defaultAvailableOutputTypes =
			new ReadOnlyCollection<string>(new string[]
			{
				DartProjectFileConstants.JavaArchiveOutputType,
			});
		private static readonly ReadOnlyCollection<string> _defaultAvailableStartupObjects =
			new ReadOnlyCollection<string>(new string[]
			{
				NotSetStartupObject,
			});

		public DartApplicationPropertyPage()
		{
			PageName = DartConfigConstants.PageNameApplication;
		}

		public new DartApplicationPropertyPagePanel PropertyPagePanel
		{
			get
			{
				return (DartApplicationPropertyPagePanel)base.PropertyPagePanel;
			}
		}

		protected override DartPropertyPagePanel CreatePropertyPagePanel()
		{
			return new DartApplicationPropertyPagePanel(this);
		}

		protected override void BindProperties()
		{
			// package name
			PropertyPagePanel.PackageName = GetConfigProperty(ProjectFileConstants.AssemblyName, _PersistStorageType.PST_PROJECT_FILE);

			// available items
			PropertyPagePanel.AvailableTargetVirtualMachines = _defaultAvailableTargetVirtualMachines;
			PropertyPagePanel.AvailableOutputTypes = _defaultAvailableOutputTypes;
			PropertyPagePanel.AvailableStartupObjects = _defaultAvailableStartupObjects;

			// selected items
			PropertyPagePanel.TargetVirtualMachine = GetConfigProperty(DartConfigConstants.TargetVM, _PersistStorageType.PST_PROJECT_FILE);
			PropertyPagePanel.OutputType = GetConfigProperty(DartConfigConstants.OutputType, _PersistStorageType.PST_PROJECT_FILE);
			PropertyPagePanel.StartupObject = GetConfigProperty(DartConfigConstants.StartupObject, _PersistStorageType.PST_PROJECT_FILE);
		}

		protected override bool ApplyChanges()
		{
			SetConfigProperty(ProjectFileConstants.AssemblyName, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.PackageName);
			SetConfigProperty(DartConfigConstants.TargetVM, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.TargetVirtualMachine);
			SetConfigProperty(DartConfigConstants.OutputType, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.OutputType);
			SetConfigProperty(DartConfigConstants.StartupObject, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.StartupObject);
			return true;
		}
	}
}
