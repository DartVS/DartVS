using Newtonsoft.Json;
namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Abstract base for classes representing a request for the Analysis Service.
	/// </summary>
	/// <typeparam name="TResponseType">The type of the response expected from the request.</typeparam>
	public abstract class Request<TResponseType>
	{
		[JsonProperty("id")]
		public string ID = "0";
		[JsonProperty("method")]
		public string Method;
	}

	/// <summary>
	/// Abstract base for classes representing a request for the Analysis Service that
	/// have a typed set of parameters in the repsonse.
	/// </summary>
	/// <typeparam name="TParamsType">The type of the parameters on the expected response.</typeparam>
	/// <typeparam name="TResponseType">The type of the response expected from the request.</typeparam>
	public abstract class Request<TParamsType, TResponseType> : Request<TResponseType>
	{
		public TParamsType @params;

		public Request(TParamsType @params)
		{
			this.@params = @params;
		}
	}
}
