using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisErrorsEvent
	{
		public string file;
		public AnalysisErrorDetails[] errors;
	}

	class AnalysisErrorDetails
	{
		public string file;
		public string errorCode;
		public int offset;
		public int length;
		public string message;

		#region Equality checks

		// Since we know these objects are serialisable, this is a quick-but-hacky way of checking for structural equality

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var serialiser = new JsonSerialiser();
			return serialiser.Serialise(this).Equals(serialiser.Serialise(obj));
		}

		public override int GetHashCode()
		{
			var serialiser = new JsonSerialiser();
			return serialiser.Serialise(this).GetHashCode();
		}

		#endregion
	}

	#endregion

	public struct AnalysisErrorsNotification
	{
		public string File { get; internal set; }
		public AnalysisError[] Errors { get; internal set; }
	}

	public struct AnalysisError
	{
		public string File { get; internal set; }
		public string ErrorCode { get; internal set; }
		public int Offset { get; internal set; }
		public int Length { get; internal set; }
		public string Message { get; internal set; }
	}

	internal static class AnalysisErrorsEventImplementation
	{
		public static void RaiseServerStatusEvent(this DartAnalysisService service, AnalysisErrorsEvent notification, EventHandler<AnalysisErrorsNotification> handler)
		{
			var errorNotification = new AnalysisErrorsNotification
			{
				File = notification.file,
				Errors = notification.errors.Select(e => new AnalysisError
				{
					File = e.file,
					ErrorCode = e.errorCode,
					Offset = e.offset,
					Length = e.length,
					Message = e.message,
				}).ToArray(),
			};

			if (handler != null)
				handler.Invoke(service, errorNotification);
		}
	}
}
