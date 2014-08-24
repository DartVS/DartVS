using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	class GoogleEnumDictionaryJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			// For some reason, JSON.NET doesn't call converters on dictionary keys! So we have to hijack the entire dictionary.
			return objectType.IsGenericType
				&& objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
				&& objectType.GetGenericArguments()[0].IsEnum
				&& !objectType.GetGenericArguments()[0].GetCustomAttributes(typeof(FlagsAttribute), false).Any();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// We don't know the real types at compile time, so we'll have to use this lame thing.
			var dictionary = (IDictionary)value;

			var mappedDictionary = dictionary
				.Keys
				.OfType<object>()
				.Select(k => new KeyValuePair<string, object>(GoogleMappingHelper.EnumToString(k), dictionary[k]))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			serializer.Serialize(writer, mappedDictionary);
		}
	}
}
