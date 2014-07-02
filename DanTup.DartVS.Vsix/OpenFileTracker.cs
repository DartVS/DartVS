using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	/// <summary>
	/// Tracks Dart files as they are opened/closed to pass to the analysis service.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export]
	public class OpenFileTracker
	{
		[Import]
		internal SVsServiceProvider ServiceProvider = null;

		DocumentEvents documentEvents;

		// There's no ConcurrentHashSet, so we'll just use a dictionary :/
		byte emptyByte = new byte();
		ConcurrentDictionary<string, byte> openDartDocuments = new ConcurrentDictionary<string, byte>();

		ReplaySubject<string[]> documentsChanged = new ReplaySubject<string[]>(1); // Keep a buffer of one, so new subscribers get the projects immediately.
		public IObservable<string[]> DocumentsChanged { get { return documentsChanged.AsObservable(); } }

		[ImportingConstructor]
		public OpenFileTracker([Import]SVsServiceProvider serviceProvider)
		{
			var dte = (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));

			// Subscribe to document events.
			documentEvents = dte.Events.DocumentEvents;
			documentEvents.DocumentOpened += TrackDocument;
			documentEvents.DocumentClosing += UntrackDocument;

			// Subscribe for existing projects already open when we were triggered.
			foreach (Document document in dte.Documents)
				TrackDocument(document);
		}

		void TrackDocument(Document document)
		{
			if (DartProjectTracker.IsDartFile(document.FullName))
				if (openDartDocuments.TryAdd(document.FullName, emptyByte))
					RaiseDocumentsChanged();
		}

		void UntrackDocument(Document document)
		{
			byte _;
			if (openDartDocuments.TryRemove(document.FullName, out _))
				RaiseDocumentsChanged();
		}

		void RaiseDocumentsChanged()
		{
			documentsChanged.OnNext(openDartDocuments.Keys.ToArray());
		}
	}
}
