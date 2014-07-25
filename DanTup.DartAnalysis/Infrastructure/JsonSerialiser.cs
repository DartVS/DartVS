using System;
using System.Linq;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Serialises and deserialises objects to/from JSON.
	/// </summary>
	class JsonSerialiser
	{
		JsonConverter[] converters = new[] {
			new GoogleEnumJsonConverter()
		};

		/// <summary>
		/// Serialises the provided object into JSON.
		/// </summary>
		/// <param name="obj">The object to serialise.</param>
		/// <returns>String of JSON representing the provided object.</returns>
		public string Serialise(object obj)
		{
			return JsonConvert.SerializeObject(obj, converters);
		}

		/// <summary>
		/// Deserialises the provided JSON into an object of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialise into.</typeparam>
		/// <param name="json">The string of JSON to deserialise.</param>
		/// <returns>A concrete object built from the provided JSON.</returns>
		public T Deserialise<T>(string json)
		{
			return (T)Deserialise(json, typeof(T));
		}

		/// <summary>
		/// Deserialises the provided JSON into an object of the provided type.
		/// </summary>
		/// <param name="json">The string of JSON to deserialise.</param>
		/// <param name="t">The Type to be deserialised into.</param>
		/// <returns>A concrete object built from the provided JSON.</returns>
		public object Deserialise(string json, Type t)
		{
			return JsonConvert.DeserializeObject(json, t, converters);
		}
	}

	class GoogleEnumJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsEnum;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException(string.Format("Cannot convert non-string value to {0}.", objectType));

			var matchingEnumValue = Enum
				.GetNames(objectType)
				.FirstOrDefault(ht => ht.ToLowerInvariant() == reader.Value.ToString().ToLowerInvariant().Replace("_", ""));

			if (matchingEnumValue == null)
				throw new JsonSerializationException(string.Format("Cannot convert value {0} to {1}.", reader.Value, objectType));

			return Enum.Parse(objectType, matchingEnumValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
