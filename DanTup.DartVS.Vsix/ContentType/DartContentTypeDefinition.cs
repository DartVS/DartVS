using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	class DartContentTypeDefinition
	{
		/// <summary>
		/// Exports the Dart content type
		/// </summary>
		[Export(typeof(ContentTypeDefinition))]
		[Name(DartConstants.ContentType)]
		[BaseDefinition("code")]
		public ContentTypeDefinition IDartContentType { get; set; }

		[Export(typeof(FileExtensionToContentTypeDefinition))]
		[ContentType(DartConstants.ContentType)]
		[FileExtension(DartConstants.FileExtension)]
		public FileExtensionToContentTypeDefinition DartFileExtension { get; set; }
	}
}
