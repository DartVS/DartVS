using System;
using System.Linq;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	class GoogleEnumJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsEnum && !objectType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException(string.Format("Cannot convert non-string value to {0}.", objectType));

			var enumValue = GoogleMappingHelper.StringToEnum(reader.Value, objectType);

			return Enum.Parse(objectType, enumValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(GoogleMappingHelper.EnumToString(value));
		}
	}
}
