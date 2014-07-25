using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Wraps the Google Dart Analysis Service providing a strongly-typed .NET
	/// interface.
	/// </summary>
	class AnalysisServiceWrapper : IDisposable
	{
		static long id = 0;
		readonly StdIOService service;
		readonly Action<Event> eventHandler;
		readonly JsonSerialiser serialiser = new JsonSerialiser();
		readonly ConcurrentDictionary<string, TaskCompletionSource<string>> pendingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

		/// <summary>
		/// Launches the Google Dart Analysis Service using the provided SDK and script.
		/// </summary>
		/// <param name="sdkFolder">The location of the Dart SDK.</param>
		/// <param name="serverScript">The location of the Dart script that runs the Analysis Service.</param>
		/// <param name="eventHandler">A handler for events raised by the Analysis Service.</param>
		public AnalysisServiceWrapper(string sdkFolder, string serverScript, Action<Event> eventHandler = null)
		{
			this.eventHandler = eventHandler;
			service = new StdIOService(Path.Combine(sdkFolder, @"bin\dart.exe"), string.Format("{0} --sdk={1}", serverScript, sdkFolder), ReceiveMessage, ReceiveError);
		}

		/// <summary>
		/// Sends a request to the Google Dart Analysis Service.
		/// </summary>
		/// <typeparam name="TResponseType">The type of the expected response.</typeparam>
		/// <param name="request">The request to send to the service.</param>
		/// <returns>A Task that will complete when the Analsysis Service responds to this request.</returns>
		public Task<TResponseType> Send<TResponseType>(Request<TResponseType> request) where TResponseType : Response
		{
			// Create a unique ID for this request.
			var requestID = Interlocked.Increment(ref id).ToString();
			request.id = requestID;

			// Create a TCS and stash it so it can be completed when the response comes through.
			var task = new TaskCompletionSource<string>();
			pendingTasks.TryAdd(requestID, task);

			// When the messages comes back, deserialise it into the correct type and just grab the result property.
			var typedTask = task.Task.ContinueWith(t => serialiser.Deserialise<TResponseType>(t.Result));

			// Finally, send the message down the wire.
			var message = serialiser.Serialise(request);
			Trace.WriteLine("STDIN:  " + message);
			service.WriteLine(message);

			return typedTask;
		}

		/// <summary>
		/// Handles receiving a message from the service via STDOUT and routes it in the correct way.
		/// </summary>
		/// <param name="message">The raw text message from the service.</param>
		void ReceiveMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return;

			Trace.WriteLine("STDOUT: " + message);

			// Parse as dict, as we don't know the full type yet.
			var response = (Dictionary<string, object>)serialiser.Deserialise<Dictionary<string, object>>(message);

			// Send the message to the most appropriate method based on the data.
			if (response.ContainsKey("id") && response.ContainsKey("error"))
				HandleErrorResponse((string)response["id"], message);
			else if (response.ContainsKey("id"))
				HandleResponse((string)response["id"], message);
			else if (response.ContainsKey("event"))
				HandleEvent((string)response["event"], message);
			else
				HandleError(message);
		}

		/// <summary>
		/// Handles receiving a message from the server via STDERR and passes it to the error handler.
		/// </summary>
		/// <param name="message">The raw text message from the service.</param>
		void ReceiveError(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return;

			Trace.WriteLine("STDERR: " + message);

			HandleError(message);
		}

		/// <summary>
		/// Handles a valid and expected response to a request.
		/// </summary>
		/// <param name="id">The ID of the request that this message is a response to.</param>
		/// <param name="message">The raw text message from the service.</param>
		void HandleResponse(string id, string message)
		{
			// Using the ID, look up the TaskCompletionSource so that we can complete the task.
			TaskCompletionSource<string> task;
			if (pendingTasks.TryGetValue(id, out task))
			{
				task.SetResult(message);
				pendingTasks.TryRemove(id, out task);
			}
			else
				HandleError(message);
		}

		/// <summary>
		/// Handles a well-formed error in response to a request.
		/// </summary>
		/// <param name="id">The ID of the request that this message is a response to.</param>
		/// <param name="message">The raw text message from the service.</param>
		void HandleErrorResponse(string id, string message)
		{
			var errorResponse = serialiser.Deserialise<ErrorResponse>(message);

			// Using the ID, look up the TaskCompletionSource so that we can complete the task.
			TaskCompletionSource<string> task;
			if (pendingTasks.TryGetValue(id, out task))
			{
				task.SetException(new ErrorResponseException(errorResponse.error.code, errorResponse.error.message));
				pendingTasks.TryRemove(id, out task);
			}
			else
				HandleError(message);
		}

		/// <summary>
		/// A mapping of event types into the types that we have to represent them.
		/// </summary>
		static readonly Dictionary<string, Type> KnownEventTypes = new Dictionary<string, Type> {
			{ "server.connected", typeof(Event) },
			{ "server.status", typeof(Event<ServerStatusEventJson>) },
			{ "analysis.errors", typeof(Event<AnalysisErrorsEvent>) },
			{ "analysis.highlights", typeof(Event<AnalysisHighlightsEvent>) },
			{ "analysis.navigation", typeof(Event<AnalysisNavigationEventJson>) },
			{ "analysis.outline", typeof(Event<AnalysisOutlineEventJson>) },
		};

		/// <summary>
		/// Handles a notification from the service that is not related to a particular request.
		/// </summary>
		/// <param name="eventType">The type of notification received.</param>
		/// <param name="message">The raw text message from the service.</param>
		void HandleEvent(string eventType, string message)
		{
			// If the user wasn't interested in events, we'll have a null handler. Just discard the message.
			if (eventHandler == null)
				return;

			if (KnownEventTypes.ContainsKey(eventType))
				eventHandler((Event)serialiser.Deserialise(message, KnownEventTypes[eventType]));
			else
				HandleError(message);
		}

		/// <summary>
		/// Handles any sort of unknown response from the server, or one that does not match up with a
		/// request that was sent.
		/// </summary>
		/// <param name="message">The raw text message from the service.</param>
		void HandleError(string message)
		{
			if (Debugger.IsAttached)
				Debugger.Break();
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
				service.Dispose();
			}
		}

		#endregion
	}
}
