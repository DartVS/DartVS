using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DanTup.DartAnalysis
{
	// This class is the entry point for consumers. In order to keep it slim, each
	// command file contains its own implementation of an extension method to handle
	// sending request/receiving response and mapping it back to a nice .NET type.

	/// <summary>
	/// Wraps the Google Dart Analysis Service providing a strongly-typed .NET
	/// interface.
	/// </summary>
	public class DartAnalysisService : IDisposable
	{
		/// <summary>
		/// The underlying service for sending requests/responses.
		/// </summary>
		internal AnalysisServiceWrapper Service { get; private set; }

		#region Events

		// Note: All subjects hold buffers for 10 seconds to avoid race conditions due to the order of things happening in VS.
		// (eg. Open Document, which adds a file to Subscriptions, fires before the taggers are created).
		// This is certainly a big hack; but I think it's better than trying to coordinate setting subscriptions from taggers for now.

		readonly ISubject<ServerStatusNotification> serverStatus = new ReplaySubject<ServerStatusNotification>(TimeSpan.FromSeconds(10));
		public IObservable<ServerStatusNotification> ServerStatusNotification { get { return serverStatus.AsObservable(); } }

		readonly ISubject<AnalysisErrors> analysisErrors = new ReplaySubject<AnalysisErrors>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisErrors> AnalysisErrorsNotification { get { return analysisErrors.AsObservable(); } }

		readonly ISubject<AnalysisHighlightsNotification> analysisHighlights = new ReplaySubject<AnalysisHighlightsNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisHighlightsNotification> AnalysisHighlightsNotification { get { return analysisHighlights.AsObservable(); } }

		readonly ISubject<AnalysisNavigationNotification> analysisNavigation = new ReplaySubject<AnalysisNavigationNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisNavigationNotification> AnalysisNavigationNotification { get { return analysisNavigation.AsObservable(); } }

		readonly ISubject<AnalysisOutlineNotification> analysisOutline = new ReplaySubject<AnalysisOutlineNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisOutlineNotification> AnalysisOutlineNotification { get { return analysisOutline.AsObservable(); } }

		#endregion

		/// <summary>
		/// Launches the Google Dart Analysis Service using the provided SDK and script.
		/// </summary>
		/// <param name="sdkFolder">The location of the Dart SDK.</param>
		/// <param name="serverScript">The location of the Dart script that runs the Analysis Service.</param>
		/// <param name="eventHandler">A handler for events raised by the Analysis Service.</param>
		public DartAnalysisService(string sdkFolder, string serverScript)
		{
			this.Service = new AnalysisServiceWrapper(sdkFolder, serverScript, HandleEvent);
		}

		void HandleEvent(Event notification)
		{
			if (notification is Event<ServerStatusEventJson>)
				TryRaiseEvent(serverStatus, () => ((Event<ServerStatusEventJson>)notification).@params.AsNotification());
			else if (notification is Event<AnalysisErrors>)
				TryRaiseEvent(analysisErrors, () => ((Event<AnalysisErrors>)notification).@params);
			else if (notification is Event<AnalysisHighlightsEventJson>)
				TryRaiseEvent(analysisHighlights, () => ((Event<AnalysisHighlightsEventJson>)notification).@params.AsNotification());
			else if (notification is Event<AnalysisNavigationEventJson>)
				TryRaiseEvent(analysisNavigation, () => ((Event<AnalysisNavigationEventJson>)notification).@params.AsNotification());
			else if (notification is Event<AnalysisOutlineEventJson>)
				TryRaiseEvent(analysisOutline, () => ((Event<AnalysisOutlineEventJson>)notification).@params.AsNotification());
		}

		void TryRaiseEvent<T>(ISubject<T> subject, Func<T> createNotification)
		{
			try
			{
				subject.OnNext(createNotification());
			}
			catch (Exception ex)
			{
				subject.OnError(ex);
			}
		}

		#region OMG DO WE STILL HAVE TO DO THIS?

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Service.Dispose();
			}
		}

		#endregion
	}
}
