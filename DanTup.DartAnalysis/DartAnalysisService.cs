using System;

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

		#region Public events

		public event EventHandler<ServerStatusNotification> ServerStatusNotification;
		public event EventHandler<AnalysisErrorsNotification> AnalysisErrorsNotification;		

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
			if (notification is Event<ServerStatusEvent>)
				this.RaiseServerStatusEvent(((Event<ServerStatusEvent>)notification).@params, this.ServerStatusNotification);
			else if (notification is Event<AnalysisErrorsEvent>)
				this.RaiseServerStatusEvent(((Event<AnalysisErrorsEvent>)notification).@params, this.AnalysisErrorsNotification);
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
