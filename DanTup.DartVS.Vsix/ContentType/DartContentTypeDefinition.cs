using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	class DartContentTypeDefinition
	{
		public const string DartContentType = "Dart";

		/// <summary>
		/// Exports the Dart content type
		/// </summary>
		[Export(typeof(ContentTypeDefinition))]
		[Name(DartContentType)]
		[BaseDefinition("code")]
		public ContentTypeDefinition IDartContentType { get; set; }

		[Export(typeof(FileExtensionToContentTypeDefinition))]
		[ContentType(DartContentType)]
		[FileExtension(".dart")]
		public FileExtensionToContentTypeDefinition DartFileExtension { get; set; }
	}
}
