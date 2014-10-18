namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using System.Runtime.InteropServices;

	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		internal static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
	}
}
