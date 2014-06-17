using System;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Represents a well-formed error returned from the Analysis Service.
	/// </summary>
	class ErrorResponseException : Exception
	{
		/// <summary>
		/// The error code returned by the Analysis Service.
		/// </summary>
		public int Code { get; private set; }

		/// <summary>
		/// The error message returned by the Analysis Service.
		/// </summary>
		public string ErrorMessage { get; private set; }

		public ErrorResponseException(int code, string message)
			: base("Error response from server: " + message)
		{
			this.Code = code;
			this.ErrorMessage = message;
		}
	}

	/// <summary>
	/// Represents a badly-formed or unexpected response from the Analysis Service.
	/// </summary>
	class UnexpectedResponseException : Exception
	{
		/// <summary>
		/// The raw response from the Analysis Service.
		/// </summary>
		public string ServerResponse { get; private set; }

		public UnexpectedResponseException(string message)
			: base("Unexpected server response: " + message)
		{
			this.ServerResponse = message;
		}
	}
}
