using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DanTup.DartAnalysis.Json;

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
		readonly string sdkFolder;

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

		readonly ISubject<AnalysisErrorsNotification> analysisErrors = new ReplaySubject<AnalysisErrorsNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisErrorsNotification> AnalysisErrorsNotification { get { return analysisErrors.AsObservable(); } }

		readonly ISubject<AnalysisHighlightsNotification> analysisHighlights = new ReplaySubject<AnalysisHighlightsNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisHighlightsNotification> AnalysisHighlightsNotification { get { return analysisHighlights.AsObservable(); } }

		readonly ISubject<AnalysisNavigationNotification> analysisNavigation = new ReplaySubject<AnalysisNavigationNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisNavigationNotification> AnalysisNavigationNotification { get { return analysisNavigation.AsObservable(); } }

		readonly ISubject<AnalysisOutlineNotification> analysisOutline = new ReplaySubject<AnalysisOutlineNotification>(TimeSpan.FromSeconds(10));
		public IObservable<AnalysisOutlineNotification> AnalysisOutlineNotification { get { return analysisOutline.AsObservable(); } }

		readonly ISubject<CompletionResultsNotification> completionResults = new ReplaySubject<CompletionResultsNotification>(TimeSpan.FromSeconds(10));
		public IObservable<CompletionResultsNotification> CompletionResultsNotification { get { return completionResults.AsObservable(); } }

		#endregion

		/// <summary>
		/// Launches the Google Dart Analysis Service using the provided SDK and script.
		/// </summary>
		/// <param name="sdkFolder">The location of the Dart SDK.</param>
		/// <param name="serverScript">The location of the Dart script that runs the Analysis Service.</param>
		/// <param name="eventHandler">A handler for events raised by the Analysis Service.</param>
		public DartAnalysisService(string sdkFolder, string serverScript)
		{
			this.sdkFolder = sdkFolder;
			this.Service = new AnalysisServiceWrapper(sdkFolder, serverScript, HandleEvent);
		}

		public string SdkFolder
		{
			get
			{
				return sdkFolder;
			}
		}

		void HandleEvent(Event notification)
		{
			if (notification is Event<ServerStatusNotification>)
				TryRaiseEvent(serverStatus, () => ((Event<ServerStatusNotification>)notification).@params);
			else if (notification is Event<AnalysisErrorsNotification>)
				TryRaiseEvent(analysisErrors, () => ((Event<AnalysisErrorsNotification>)notification).@params);
			else if (notification is Event<AnalysisHighlightsNotification>)
				TryRaiseEvent(analysisHighlights, () => ((Event<AnalysisHighlightsNotification>)notification).@params);
			else if (notification is Event<AnalysisNavigationNotification>)
				TryRaiseEvent(analysisNavigation, () => ((Event<AnalysisNavigationNotification>)notification).@params);
			else if (notification is Event<AnalysisOutlineNotification>)
				TryRaiseEvent(analysisOutline, () => ((Event<AnalysisOutlineNotification>)notification).@params);
			else if (notification is Event<CompletionResultsNotification>)
				TryRaiseEvent(completionResults, () => ((Event<CompletionResultsNotification>)notification).@params);
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
	}
}
