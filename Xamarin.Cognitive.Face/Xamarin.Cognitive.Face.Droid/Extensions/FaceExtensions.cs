using System;
using System.Collections.Generic;
using Android.Graphics;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static partial class FaceExtensions
	{
		public static string GetFormattedGroupName (this PersonGroup personGroup)
		{
			return string.Format ("{0} (Person count: {1})", personGroup.Name, personGroup.People?.Count);
		}


		/// <summary>
		/// Creates a thumbnail image for a given Face.
		/// </summary>
		/// <returns>The thumbnail image.</returns>
		/// <param name="face">The Face to generate a thumbnail for.</param>
		/// <param name="sourceImage">The source image or photo to crop the thumbnail from.</param>
		public static Bitmap CreateThumbnail (this Model.Face face, Bitmap sourceImage)
		{
			var largeFaceRect = face.FaceRectangle.CalculateLargeFaceRectangle (sourceImage);

			return sourceImage.Crop (largeFaceRect);
		}


		/// <summary>
		/// Generates the thumbnail images for a given set of Faces from a given photo.
		/// </summary>
		/// <returns>The thumbnail images.</returns>
		/// <param name="faces">The set of faces to generate thumbnail images for.</param>
		/// <param name="photo">The source image or photo to crop the thumbnails from.</param>
		/// <param name="thumbnailList">Optional existing thumbnail list to append to.</param>
		public static List<Bitmap> GenerateThumbnails (this List<Model.Face> faces, Bitmap photo, List<Bitmap> thumbnailList = null)
		{
			var faceThumbnails = thumbnailList ?? new List<Bitmap> ();

			if (faces != null)
			{
				foreach (var face in faces)
				{
					try
					{
						faceThumbnails.Add (face.CreateThumbnail (photo));
					}
					catch (Exception ex)
					{
						Log.Error (ex);
						//TODO: add stock photo if/when this fails?
					}
				}
			}

			return faceThumbnails;
		}


		/// <summary>
		/// Saves the thumbnail image using the Face's current thumbnail path.
		/// </summary>
		/// <param name="face">Face.</param>
		/// <param name="thumbnail">Thumbnail image.</param>
		public static void SaveThumbnail (this Model.Face face, Bitmap thumbnail)
		{
			face.UpdateThumbnailPath ();
			thumbnail.SaveAsJpeg (face.ThumbnailPath);
		}


		/// <summary>
		/// Gets the thumbnail image for the given Face.  This assumes the thumbnail has already been saved using the <see cref="SaveThumbnail"/> method.
		/// </summary>
		/// <returns>The thumbnail image.</returns>
		/// <param name="face">Face.</param>
		public static Bitmap GetThumbnailImage (this Model.Face face)
		{
			if (!string.IsNullOrEmpty (face.ThumbnailPath))
			{
				return BitmapFactory.DecodeFile (face.ThumbnailPath);
			}

			return null;
		}
	}
}