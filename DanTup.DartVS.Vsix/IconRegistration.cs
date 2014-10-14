using System.IO;
using Microsoft.Win32;

namespace DanTup.DartVS
{
	// Borrowed from WebEssentials...
	internal static class IconRegistration
	{
		static string iconFolder = Path.Combine(Path.GetDirectoryName(typeof(IconRegistration).Assembly.Location), "Resources");

		public static void RegisterIcons()
		{
			using (RegistryKey classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", true))
			{
				if (classes == null)
					return;

				AddIcon(classes, "Dart.ico", ".dart");
			}
		}

		private static void AddIcon(RegistryKey classes, string iconName, params string[] extensions)
		{
			foreach (string extension in extensions)
			{
				using (RegistryKey key = classes.CreateSubKey(extension + "\\DefaultIcon"))
				{
					key.SetValue(string.Empty, Path.Combine(iconFolder, iconName));
				}
			}
		}
	}
}
