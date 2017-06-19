using System;
using UIKit;
using Xamarin.Cognitive.Face.Model;
using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static partial class FaceExtensions
	{
		/// <summary>
		/// Creates a thumbnail image for a given Face.
		/// </summary>
		/// <returns>The thumbnail image.</returns>
		/// <param name="face">The Face to generate a thumbnail for.</param>
		/// <param name="sourceImage">The source image or photo to crop the thumbnail from.</param>
		public static UIImage CreateThumbnail (this Model.Face face, UIImage sourceImage)
		{
			return sourceImage.Crop (face.FaceRectangle);
		}


		/// <summary>
		/// Generates the thumbnail images for a given set of Faces from a given photo.
		/// </summary>
		/// <returns>The thumbnail images.</returns>
		/// <param name="faces">The set of faces to generate thumbnail images for.</param>
		/// <param name="photo">The source image or photo to crop the thumbnails from.</param>
		/// <param name="thumbnailList">Optional existing thumbnail list to append to.</param>
		public static List<UIImage> GenerateThumbnails (this List<Model.Face> faces, UIImage photo, List<UIImage> thumbnailList = null)
		{
			var faceThumbnails = thumbnailList ?? new List<UIImage> ();

			if (faces != null)
			{
				foreach (var face in faces)
				{
					try
					{
						//var largeFaceRect = face.FaceRectangle.CalculateLargeFaceRectangle (photo);

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
		public static void SaveThumbnail (this Model.Face face, UIImage thumbnail)
		{
			face.UpdateThumbnailPath ();
			thumbnail.SaveAsJpeg (face.ThumbnailPath);
		}


		public static void SaveThumbnailFromSource (this Model.Face face, UIImage sourceImage)
		{
			using (var thumbnail = face.CreateThumbnail (sourceImage))
			{
				face.SaveThumbnail (thumbnail);
			}
		}


		/// <summary>
		/// Gets the thumbnail image for the given Face.  This assumes the thumbnail has already been saved using the <see cref="SaveThumbnail"/> method.
		/// </summary>
		/// <returns>The thumbnail image.</returns>
		/// <param name="face">Face.</param>
		public static UIImage GetThumbnailImage (this Model.Face face)
		{
			return UIImage.FromFile (face.ThumbnailPath);
		}


		public static Dictionary<Model.Face, UIImage> GetThumbnails (this GroupResult groupResult, Func<Model.Face, UIImage> imageProvider)
		{
			var faceImages = new Dictionary<Model.Face, UIImage> ();

			foreach (var faceGroup in groupResult.Groups)
			{
				foreach (var face in faceGroup.Faces)
				{
					faceImages.Add (face, imageProvider (face));
				}
			}

			foreach (var face in groupResult.MessyGroup?.Faces)
			{
				faceImages.Add (face, imageProvider (face));
			}

			return faceImages;
		}
	}
}