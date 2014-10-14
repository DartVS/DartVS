using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DanTup.DartVS
{
	class CompletionController : DartOleCommandTarget<VSConstants.VSStd2KCmdID>
	{
		ICompletionBroker broker;
		ICompletionSession currentSession;

		public CompletionController(ITextDocumentFactoryService textDocumentFactory, IVsTextView textViewAdapter, IWpfTextView textView, ICompletionBroker broker, DartAnalysisServiceFactory analysisServiceFactory)
			: base(textDocumentFactory, textViewAdapter, textView, analysisServiceFactory, VSConstants.VSStd2KCmdID.AUTOCOMPLETE, VSConstants.VSStd2KCmdID.COMPLETEWORD, VSConstants.VSStd2KCmdID.RETURN, VSConstants.VSStd2KCmdID.TAB, VSConstants.VSStd2KCmdID.Cancel, VSConstants.VSStd2KCmdID.TYPECHAR, VSConstants.VSStd2KCmdID.BACKSPACE)
		{
			this.broker = broker;
		}

		protected override void Exec(uint nCmdID, IntPtr pvaIn)
		{
			bool handled;
			switch ((VSConstants.VSStd2KCmdID)nCmdID)
			{
				case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
				case VSConstants.VSStd2KCmdID.COMPLETEWORD:
					handled = StartSession();
					break;
				case VSConstants.VSStd2KCmdID.RETURN:
					handled = Complete(false);
					break;
				case VSConstants.VSStd2KCmdID.TAB:
					handled = Complete(true);
					break;
				case VSConstants.VSStd2KCmdID.CANCEL:
					handled = Cancel();
					break;
				case VSConstants.VSStd2KCmdID.TYPECHAR:
					char ch = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
					if (ch == '.')
						StartSession();
					break;
				case VSConstants.VSStd2KCmdID.BACKSPACE:
					StartSession();
					break;
			}
		}

		bool StartSession()
		{
			if (currentSession != null)
				return false;

			SnapshotPoint caret = textView.Caret.Position.BufferPosition;
			ITextSnapshot snapshot = caret.Snapshot;

			if (!broker.IsCompletionActive(textView))
			{
				currentSession = broker.CreateCompletionSession(textView, snapshot.CreateTrackingPoint(caret, PointTrackingMode.Positive), true);
			}
			else
			{
				currentSession = broker.GetSessions(textView)[0];
			}
			currentSession.Dismissed += (sender, args) => currentSession = null;

			currentSession.Start();

			return true;
		}

		bool Cancel()
		{
			if (currentSession == null)
				return false;

			currentSession.Dismiss();

			return true;
		}

		bool Complete(bool force)
		{
			if (currentSession == null)
				return false;

			if (!currentSession.SelectedCompletionSet.SelectionStatus.IsSelected && !force)
			{
				currentSession.Dismiss();
				return false;
			}
			else
			{
				currentSession.Commit();
				return true;
			}
		}
	}
}
