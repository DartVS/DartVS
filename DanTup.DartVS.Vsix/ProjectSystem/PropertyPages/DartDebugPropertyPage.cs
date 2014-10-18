namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
	using System;
	using System.Runtime.InteropServices;
	using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

	[ComVisible(true)]
	[Guid(DartProjectConstants.DebugPropertyPageGuidString)]
	public class DartDebugPropertyPage : DartPropertyPage
	{
		public DartDebugPropertyPage()
		{
			PageName = DartConfigConstants.PageNameDebug;
		}

		public new DartDebugPropertyPagePanel PropertyPagePanel
		{
			get
			{
				return (DartDebugPropertyPagePanel)base.PropertyPagePanel;
			}
		}

		protected override DartPropertyPagePanel CreatePropertyPagePanel()
		{
			return new DartDebugPropertyPagePanel(this);
		}

		protected override void BindProperties()
		{
			if (ProjectManager != null)
				ProjectManager.SharedBuildOptions.Debug = PropertyPagePanel;

			StartAction startAction;
			if (!Enum.TryParse(GetConfigProperty(DartConfigConstants.DebugStartAction, _PersistStorageType.PST_USER_FILE), out startAction))
				startAction = StartAction.Class;

			PropertyPagePanel.StartAction = startAction;
			PropertyPagePanel.StartClass = GetConfigProperty(DartConfigConstants.DebugStartClass, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.StartProgram = GetConfigProperty(DartConfigConstants.DebugStartProgram, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.StartBrowserUrl = GetConfigProperty(DartConfigConstants.DebugStartBrowserUrl, _PersistStorageType.PST_USER_FILE);

			PropertyPagePanel.ExtraArguments = GetConfigProperty(DartConfigConstants.DebugExtraArgs, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.WorkingDirectory = GetConfigProperty(DartConfigConstants.DebugWorkingDirectory, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.UseRemoteMachine = GetConfigPropertyBoolean(DartConfigConstants.DebugUseRemoteMachine, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.RemoteMachineName = GetConfigProperty(DartConfigConstants.DebugRemoteMachineName, _PersistStorageType.PST_USER_FILE);

			PropertyPagePanel.VirtualMachineArguments = GetConfigProperty(DartConfigConstants.DebugJvmArguments, _PersistStorageType.PST_USER_FILE);
			PropertyPagePanel.AgentArguments = GetConfigProperty(DartConfigConstants.DebugAgentArguments, _PersistStorageType.PST_USER_FILE);
		}

		protected override bool ApplyChanges()
		{
			SetConfigProperty(DartConfigConstants.DebugStartAction, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.StartAction.ToString());
			SetConfigProperty(DartConfigConstants.DebugStartClass, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.StartClass);
			SetConfigProperty(DartConfigConstants.DebugStartProgram, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.StartProgram);
			SetConfigProperty(DartConfigConstants.DebugStartBrowserUrl, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.StartBrowserUrl);

			SetConfigProperty(DartConfigConstants.DebugExtraArgs, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.ExtraArguments);
			SetConfigProperty(DartConfigConstants.DebugWorkingDirectory, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.WorkingDirectory);
			SetConfigProperty(DartConfigConstants.DebugUseRemoteMachine, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.UseRemoteMachine);
			SetConfigProperty(DartConfigConstants.DebugRemoteMachineName, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.RemoteMachineName);

			SetConfigProperty(DartConfigConstants.DebugJvmArguments, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.VirtualMachineArguments);
			SetConfigProperty(DartConfigConstants.DebugAgentArguments, _PersistStorageType.PST_USER_FILE, PropertyPagePanel.AgentArguments);

			return true;
		}
	}
}
