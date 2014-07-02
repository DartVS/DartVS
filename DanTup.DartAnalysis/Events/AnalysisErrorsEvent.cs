using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisErrorsEvent
	{
		public string file = null;
		public AnalysisErrorDetails[] errors = null;
	}

	class AnalysisErrorDetails
	{
		public string errorCode = null;
		public string severity = null;
		public string type = null;
		public AnalysisErrorLocationDetails location = null;
		public string message = null;

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

	class AnalysisErrorLocationDetails
	{
		public string file = null;
		public int offset = 0;
		public int length = 0;
		public int startLine = 0;
		public int startColumn = 0;
	}

	#endregion

	public struct AnalysisErrorsNotification
	{
		public string File { get; internal set; }
		public AnalysisError[] Errors { get; internal set; }
	}

	public struct AnalysisError
	{
		public string ErrorCode { get; internal set; }
		public AnalysisErrorSeverity Severity { get; internal set; }
		public AnalysisErrorType Type { get; internal set; }
		public AnalysisErrorLocation Location { get; internal set; }
		public string Message { get; internal set; }
	}

	public struct AnalysisErrorLocation
	{
		public string File { get; internal set; }
		public int Offset { get; internal set; }
		public int Length { get; internal set; }
		public int StartLine { get; internal set; }
		public int StartColumn { get; internal set; }
	}

	public enum AnalysisErrorSeverity
	{
		Warning
	}

	public enum AnalysisErrorType
	{
		StaticWarning
	}

	internal static class AnalysisErrorsEventImplementation
	{
		static AnalysisErrorSeverity[] ErrorSeverities = Enum.GetValues(typeof(AnalysisErrorSeverity)).Cast<AnalysisErrorSeverity>().ToArray();
		static AnalysisErrorType[] ErrorTypes = Enum.GetValues(typeof(AnalysisErrorType)).Cast<AnalysisErrorType>().ToArray();

		public static AnalysisErrorsNotification AsNotification(this AnalysisErrorsEvent notification)
		{
			return new AnalysisErrorsNotification
			{
				File = notification.file,
				Errors = notification.errors.Select(e => new AnalysisError
				{
					ErrorCode = e.errorCode,
					Severity = ErrorSeverities.FirstOrDefault(s => s.ToString().ToLowerInvariant() == e.severity.ToLowerInvariant().Replace("_", "")),
					Type = ErrorTypes.FirstOrDefault(et => et.ToString().ToLowerInvariant() == e.type.ToLowerInvariant().Replace("_", "")),
					Location = new AnalysisErrorLocation
					{
						File = e.location.file,
						Offset = e.location.offset,
						Length = e.location.length,
						StartLine = e.location.startLine,
						StartColumn = e.location.startColumn,
					},
					Message = e.message,
				}).ToArray(),
			};
		}
	}
}
