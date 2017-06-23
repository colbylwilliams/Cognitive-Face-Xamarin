using System;
using System.IO;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static partial class FaceExtensions
	{
		static string DocsDir;

		static FaceExtensions ()
		{
			DocsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}


		/// <summary>
		/// Updates the thumbnail path of the Face.  This will use the current Id to generate the thumbnail path for this Face.
		/// </summary>
		/// <param name="face">The <see cref="Face"/>.</param>
		public static void UpdateThumbnailPath (this Model.Face face)
		{
			var filePath = Path.Combine (DocsDir, face.FileName);
			face.ThumbnailPath = filePath;
		}
	}
}