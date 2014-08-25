using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DanTup.DartAnalysis;
using DanTup.DartAnalysis.Json;
using EnvDTE;

namespace DanTup.DartVS
{
	public class DartFileChangeTracker
	{
		DTE dte;
		DartAnalysisService analysisService;
		TextEditorEvents textEditorEvents;

		Subject<Document> documentChanged = new Subject<Document>();

		public DartFileChangeTracker(DTE dte, DartAnalysisService analysisService)
		{
			this.dte = dte;
			this.analysisService = analysisService;

			textEditorEvents = dte.Events.TextEditorEvents; // We need to keep this, or it'll be disposed!

			textEditorEvents.LineChanged += (s, e, h) => documentChanged.OnNext(s.Parent.Parent);

			documentChanged
				// TODO: Check whether this is safe? What if the user does find/replace across multiple files?
				// TODO: Could we group these instead of just taking the "last" (Throttle's behaviour) and send them in one go?
				.Throttle(TimeSpan.FromMilliseconds(100))
				.Subscribe(DocumentChanged);
		}

		void DocumentChanged(Document document)
		{
			// Sometimes this is null :-/
			if (document == null)
				return;

			var path = document.FullName;

			var textDoc = document.Object("") as TextDocument;
			var fileContents = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);

			// TODO: Optimise this to use ChangeContentOverlay on subsequent updates.
			var change = new AddContentOverlay
			{
				Type = "add",
				Content = fileContents
			};

			analysisService.UpdateContent(path, change);
		}
	}
}
