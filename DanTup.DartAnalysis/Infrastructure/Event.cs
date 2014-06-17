using System.Web.Script.Serialization;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Represents an event raised by the Analysis Service that has
	/// type parameters.
	/// </summary>
	/// <typeparam name="TParams">The type of the paramters for this event.</typeparam>
	class Event<TParams> : Event
	{
		public TParams @params = default(TParams);
	}

	/// <summary>
	/// Represents an event raised by the Analysis Service.
	/// </summary>
	class Event
	{
		public string @event = null;

		#region Equality checks

		// Since we know these objects are serialisable, this is a quick-but-hacky way of checking for structural equality.
		// TODO: Make this better (or convert to F#? ;))

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var serialiser = new JavaScriptSerializer();
			return serialiser.Serialize(this).Equals(serialiser.Serialize(obj));
		}

		public override int GetHashCode()
		{
			var serialiser = new JavaScriptSerializer();
			return serialiser.Serialize(this).GetHashCode();
		}

		#endregion
	}
}
