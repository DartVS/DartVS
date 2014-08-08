using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Threading;
using DanTup.DartAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	public class Danny { }

	class NavigationDropdown : IVsDropdownBarClient
	{
		static ImageList images;
		static readonly int ClassImageIndex, MethodImageIndex, FieldImageIndex;
		static NavigationDropdown()
		{
			var asm = Assembly.GetExecutingAssembly();

			images = new ImageList();
			images.ImageSize = new Size(16, 16);
			ClassImageIndex = 0;
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Class.png")));
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Class_Private.png")));
			MethodImageIndex = 2;
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Method.png")));
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Method_Private.png")));
			FieldImageIndex = 4;
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Field.png")));
			images.Images.Add(Image.FromStream(asm.GetManifestResourceStream("DanTup.DartVS.Resources.Field_Private.png")));
		}


		IVsDropdownBar dropdown;
		DartAnalysisService analysisService;
		string file;
		IWpfTextView wpfTextView;

		Dispatcher dispatcher;

		IDisposable subscription;
		AnalysisOutline[] topLevelItems = new AnalysisOutline[0];
		AnalysisOutline[] secondLevelItems = new AnalysisOutline[0];

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

		void UpdateSourceData(AnalysisOutlineEvent notification)
		{
			topLevelItems = (notification.Outline.Children ?? new AnalysisOutline[0]).ToArray();

			SelectTopLevelItemForPosition(wpfTextView.Caret.Position.BufferPosition.Position);
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
			{
				RefreshComboOnUiThread(0, itemToSelect.Index);
				if (itemToSelect.Item.Children != null)
					secondLevelItems = itemToSelect.Item.Children.ToArray();
				else
					secondLevelItems = new AnalysisOutline[0];
				SelectSecondLevelItemForPosition(caretPosition);
			}
			else
				RefreshComboOnUiThread(0, -1);
		}

		void SelectSecondLevelItemForPosition(int caretPosition)
		{
			var itemToSelect = secondLevelItems
				.Select((item, index) => new { Item = item, Index = index }) // Add indexes, since that's ultimately what we need!
				.FirstOrDefault(o => o.Item.Offset <= caretPosition && o.Item.Offset + o.Item.Length >= caretPosition); // Find the first item within the range

			if (itemToSelect != null)
				RefreshComboOnUiThread(1, itemToSelect.Index);
			else
				RefreshComboOnUiThread(1, -1);
		}

		void CenterAndFocus(int index, int length)
		{
			wpfTextView.Caret.MoveTo(new SnapshotPoint(wpfTextView.TextBuffer.CurrentSnapshot, index));

			wpfTextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(wpfTextView.TextBuffer.CurrentSnapshot, index, length), EnsureSpanVisibleOptions.AlwaysCenter);

			((System.Windows.Controls.Control)wpfTextView).Focus();
		}

		string BuildName(AnalysisOutline outline)
		{
			return outline.Element.Parameters == null ? outline.Element.Name : outline.Element.Name + outline.Element.Parameters;
		}

		int GetImageIndex(AnalysisElement element)
		{
			var index = -1;
			switch (element.Kind)
			{
				case ElementKind.Class:
					index = ClassImageIndex;
					break;
				case ElementKind.Method:
				case ElementKind.Function:
				case ElementKind.Constructor:
					index = MethodImageIndex;
					break;
				case ElementKind.Field:
					index = FieldImageIndex;
					break;
			}

			if (index > -1 && element.Name.StartsWith("_"))
				index++;

			return index;
		}

		#region IVsDropdownBarClient Members

		public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
		{
			switch (iCombo)
			{
				case 0:
					pcEntries = (uint)topLevelItems.Length;
					break;
				case 1:
					pcEntries = (uint)secondLevelItems.Length;
					break;
				default:
					pcEntries = 0;
					break;
			}

			puEntryType = (uint)(DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_IMAGE);
			phImageList = images.Handle;

			return VSConstants.S_OK;
		}

		public int GetComboTipText(int iCombo, out string pbstrText)
		{
			// TODO: Can we get DartDoc here?
			pbstrText = "";
			return VSConstants.E_NOTIMPL;
		}

		public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
		{
			pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;
			return VSConstants.S_OK;
		}

		public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex)
		{
			switch (iCombo)
			{
				case 0:
					if (iIndex < topLevelItems.Length)
						piImageIndex = GetImageIndex(topLevelItems[iIndex].Element);
					else
						piImageIndex = -1;
					break;
				case 1:
					if (iIndex < secondLevelItems.Length)
						piImageIndex = GetImageIndex(secondLevelItems[iIndex].Element);
					else
						piImageIndex = -1;
					break;
				default:
					piImageIndex = -1;
					break;
			}

			return VSConstants.S_OK;
		}

		public int GetEntryText(int iCombo, int iIndex, out string ppszText)
		{
			switch (iCombo)
			{
				case 0:
					if (iIndex < topLevelItems.Length)
						ppszText = BuildName(topLevelItems[iIndex]);
					else
						ppszText = ""; // Likely an old notification :(
					break;
				case 1:
					if (iIndex < secondLevelItems.Length)
						ppszText = BuildName(secondLevelItems[iIndex]);
					else
						ppszText = ""; // Likely an old notification :(
					break;
				default:
					ppszText = "";
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
			switch (iCombo)
			{
				case 0:
					if (iIndex < topLevelItems.Length)
					{
						var selectedItem = topLevelItems[iIndex].Element;
						CenterAndFocus(selectedItem.Location.Offset, selectedItem.Location.Length);
					}
					break;
				case 1:
					if (iIndex < secondLevelItems.Length)
					{
						var selectedItem = secondLevelItems[iIndex].Element;
						CenterAndFocus(selectedItem.Location.Offset, selectedItem.Location.Length);
					}
					break;
				default:
					break;
			}

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

		#endregion
	}
}
