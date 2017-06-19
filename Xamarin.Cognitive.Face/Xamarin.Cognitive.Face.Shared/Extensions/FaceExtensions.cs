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


		public static void UpdateThumbnailPath (this Model.Face face)
		{
			var filePath = Path.Combine (DocsDir, face.FileName);
			face.ThumbnailPath = filePath;
		}
	}
}