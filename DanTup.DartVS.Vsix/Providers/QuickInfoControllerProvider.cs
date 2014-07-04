using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(IIntellisenseControllerProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	class QuickInfoControllerProvider : IIntellisenseControllerProvider
	{
		[Import]
		internal IQuickInfoBroker QuickInfoBroker { get; set; }

		public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
		{
			return new QuickInfoController(textView, subjectBuffers, this);
		}
	}

	class QuickInfoController : IIntellisenseController
	{
		ITextView textView;
		IList<ITextBuffer> subjectBuffers;
		QuickInfoControllerProvider provider;
		IQuickInfoSession session;

		public QuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, QuickInfoControllerProvider provider)
		{
			this.textView = textView;
			this.subjectBuffers = subjectBuffers;
			this.provider = provider;

			textView.MouseHover += this.OnTextViewMouseHover;
		}

		private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
		{
			// I don't really know what this is all for; but it's in the MSDN sample...
			// http://msdn.microsoft.com/en-us/library/vstudio/ee197646(v=vs.120).aspx
			SnapshotPoint? point = textView.BufferGraph.MapDownToFirstMatch(new SnapshotPoint(textView.TextSnapshot, e.Position), PointTrackingMode.Positive, snapshot => subjectBuffers.Contains(snapshot.TextBuffer), PositionAffinity.Predecessor);

			if (point != null)
			{
				ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
				PointTrackingMode.Positive);

				if (!provider.QuickInfoBroker.IsQuickInfoActive(textView))
				{
					session = provider.QuickInfoBroker.TriggerQuickInfo(textView, triggerPoint, true);
				}
			}
		}

		public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
		{
		}

		public void Detach(ITextView txt)
		{
			if (textView == txt)
			{
				textView.MouseHover -= this.OnTextViewMouseHover;
				textView = null;
			}
		}

		public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
		{
		}
	}

}
