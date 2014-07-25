using Newtonsoft.Json;
namespace DanTup.DartAnalysis
{
	public struct AnalysisErrorsEvent
	{
		[JsonProperty]
		public string File { get; internal set; }

		[JsonProperty]
		public AnalysisError[] Errors { get; internal set; }
	}

	public struct AnalysisError
	{
		[JsonProperty]
		public AnalysisErrorSeverity Severity { get; internal set; }

		[JsonProperty]
		public AnalysisErrorType Type { get; internal set; }

		[JsonProperty]
		public AnalysisLocation Location { get; internal set; }

		[JsonProperty]
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
}
