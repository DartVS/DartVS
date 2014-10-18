namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using Microsoft.Build.Utilities;

	public static class CommandLineBuilderExtensions
	{
		public static void AppendSwitchIfNotNullOrEmpty(this CommandLineBuilder commandLine, string switchName, string parameter)
		{
			if (commandLine == null)
				throw new ArgumentNullException("commandLine");
			if (switchName == null)
				throw new ArgumentNullException("switchName");
			if (string.IsNullOrEmpty(switchName))
				throw new ArgumentException("switchName cannot be empty", "switchName");

			if (!string.IsNullOrEmpty(parameter))
				commandLine.AppendSwitchIfNotNull(switchName, parameter);
		}
	}
}
