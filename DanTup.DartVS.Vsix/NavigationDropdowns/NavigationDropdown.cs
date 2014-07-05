using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Threading;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class NavigationDropdown : IVsDropdownBarClient
	{
		IVsDropdownBar dropdown;
		DartAnalysisService analysisService;
		string file;
		IWpfTextView wpfTextView;

		Dispatcher dispatcher;

		IDisposable subscription;
		AnalysisOutline[] topLevelItems = new AnalysisOutline[0];

		public NavigationDropdown(DartAnalysisService analysisService, string file, IWpfTextView wpfTextView)
		{
			this.analysisService = analysisService;
			this.file = file;
			this.wpfTextView = wpfTextView;

			this.wpfTextView.Caret.PositionChanged += CaretPositionChanged;

			// Capture dispatcher so we can call RefreshCombo on the correct thread.
			dispatcher = Dispatcher.CurrentDispatcher;

			// Subscribe to outline updates for this file
			subscription = this.analysisService.AnalysisOutlineNotification.Where(en => en.File == file).Subscribe(UpdateSourceData);
		}

		internal void Unregister()
		{
			this.wpfTextView.Caret.PositionChanged -= CaretPositionChanged;
			subscription.Dispose();
		}

		void UpdateSourceData(AnalysisOutlineNotification notification)
		{
			topLevelItems = notification.Outline.Children.ToArray();

			RefreshComboOnUiThread(0, 0);
		}

		void RefreshComboOnUiThread(int combo, int selectedItem)
		{
			Action refreshCombo = () =>
			{
				dropdown.RefreshCombo(combo, selectedItem);
			};

			dispatcher.BeginInvoke(refreshCombo, DispatcherPriority.Background);
		}

		void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
		{
			SelectTopLevelItemForPosition(e.NewPosition.BufferPosition.Position);
		}

		void SelectTopLevelItemForPosition(int caretPosition)
		{
			var itemToSelect = topLevelItems
				.Select((item, index) => new { Item = item, Index = index }) // Add indexes, since that's ultimately what we need!
				.FirstOrDefault(o => o.Item.Offset <= caretPosition && o.Item.Offset + o.Item.Length >= caretPosition); // Find the first item within the range

			if (itemToSelect != null)
				RefreshComboOnUiThread(0, itemToSelect.Index);
		}

		public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
		{
			switch (iCombo)
			{
				case 0:
					pcEntries = (uint)topLevelItems.Length;
					break;
				case 1:
					pcEntries = 0;
					break;
				default:
					pcEntries = 0;
					break;
			}

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
			switch (iCombo)
			{
				case 0:
					if (iIndex < topLevelItems.Length)
						ppszText = topLevelItems[iIndex].Element.Name;
					else
						ppszText = ""; // Likely an old notification :(
					break;
				case 1:
					// TODO: Stuff here
					ppszText = "INFO FOR " + file;
					break;
				default:
					// TODO: Stuff here
					ppszText = "INFO FOR " + file;
					break;
			}

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
