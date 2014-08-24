using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis
{
	/// <summary>
	/// Maps values from enum CONSTANTS to .NET EnumValues.
	/// </summary>
	static class GoogleMappingHelper
	{
		static Regex uppercaseCharactersExcludingFirst = new Regex("([A-Z])", RegexOptions.Compiled);

		public static string EnumToString(object value)
		{
			var enumValue = value.ToString();

			// Prefix any caps with underscores (except first).
			enumValue = enumValue[0] + uppercaseCharactersExcludingFirst.Replace(enumValue.Substring(1), "_$1");

			// Uppercase the whole string.
			enumValue = enumValue.ToUpper();

			return enumValue;
		}

		public static string StringToEnum(object value, Type objectType)
		{
			var wantedEnumValue = value.ToString().Replace("_", "");

			var matchingEnumValue = Enum
				.GetNames(objectType)
				.FirstOrDefault(ht => string.Equals(ht, wantedEnumValue, StringComparison.OrdinalIgnoreCase));

			if (matchingEnumValue == null)
				throw new JsonSerializationException(string.Format("Cannot convert value {0} to {1}.", value, objectType));

			return matchingEnumValue;
		}
	}
}
