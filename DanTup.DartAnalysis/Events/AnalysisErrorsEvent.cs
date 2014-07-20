using System;
using System.Linq;

namespace DanTup.DartAnalysis
{
	#region JSON deserialisation objects

	class AnalysisErrorsEventJson
	{
		public string file = null;
		public AnalysisErrorJson[] errors = null;
	}

	class AnalysisErrorJson
	{
		public string severity = null;
		public string type = null;
		public AnalysisLocationJson location = null;
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

	#endregion

	public struct AnalysisErrorsNotification
	{
		public string File { get; internal set; }
		public AnalysisError[] Errors { get; internal set; }
	}

	public struct AnalysisError
	{
		public AnalysisErrorSeverity Severity { get; internal set; }
		public AnalysisErrorType Type { get; internal set; }
		public AnalysisLocation Location { get; internal set; }
		public string Message { get; internal set; }
	}

	// TODO: Add all of these
	public enum AnalysisErrorSeverity
	{
		None,
		Info,
		Warning,
		Error
	}

	// TODO: Add all of these (inc. comments what they mean!)
	public enum AnalysisErrorType
	{
		TODO,
		Hint,
		CompileTimeError,
		PubSuggestion,
		StaticWarning,
		StaticTypeWarning,
		SyntacticError,
		Angular,
		Polymer
	}

	internal static class AnalysisErrorsEventImplementation
	{
		static AnalysisErrorSeverity[] ErrorSeverities = Enum.GetValues(typeof(AnalysisErrorSeverity)).Cast<AnalysisErrorSeverity>().ToArray();
		static AnalysisErrorType[] ErrorTypes = Enum.GetValues(typeof(AnalysisErrorType)).Cast<AnalysisErrorType>().ToArray();

		public static AnalysisErrorsNotification AsNotification(this AnalysisErrorsEventJson notification)
		{
			return new AnalysisErrorsNotification
			{
				File = notification.file,
				Errors = notification.errors.Select(e => new AnalysisError
				{
					Severity = ErrorSeverities.FirstOrDefault(s => s.ToString().ToLowerInvariant() == e.severity.ToLowerInvariant().Replace("_", "")),
					Type = ErrorTypes.FirstOrDefault(et => et.ToString().ToLowerInvariant() == e.type.ToLowerInvariant().Replace("_", "")),
					Location = new AnalysisLocation
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
