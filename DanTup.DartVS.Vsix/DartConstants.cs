namespace DanTup.DartVS
{
	internal static class DartConstants
	{
		/* The language name (used for the language service) and content type must be the same due to the way Visual
		 * Studio internally registers file extensions and content types.
		 */
		public const string LanguageName = "Dart";
		public const string ContentType = LanguageName;

		public const string FileExtension = ".dart";
	}
}
