using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class NavigationDropdown : IVsDropdownBarClient
	{
		IVsDropdownBar dropdown;
		string file;

		public NavigationDropdown(string file)
		{
			this.file = file;
		}

		public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
		{
			pcEntries = 1;
			puEntryType = (uint)DROPDOWNENTRYTYPE.ENTRY_TEXT;
			phImageList = IntPtr.Zero;

			return VSConstants.S_OK;
		}

		public int GetComboTipText(int iCombo, out string pbstrText)
		{
			// TODO: Tooltip
			pbstrText = "TODO: Tooltip here";
			return VSConstants.S_OK;
		}

		public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
		{
			pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;
			return VSConstants.S_OK;
		}

		public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex)
		{
			piImageIndex = 0;
			return VSConstants.S_OK;
		}

		public int GetEntryText(int iCombo, int iIndex, out string ppszText)
		{
			// TODO: Stuff here
			ppszText = "INFO FOR " + file;
			return VSConstants.S_OK;
		}

		public int OnComboGetFocus(int iCombo)
		{
			return VSConstants.S_OK;
		}

		public int OnItemChosen(int iCombo, int iIndex)
		{
			return VSConstants.S_OK;
		}

		public int OnItemSelected(int iCombo, int iIndex)
		{
			return VSConstants.S_OK;
		}

		public int SetDropdownBar(IVsDropdownBar pDropdownBar)
		{
			dropdown = pDropdownBar;
			return VSConstants.S_OK;
		}
	}
}
