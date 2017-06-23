using System;
using System.Collections.Generic;
using Android.Graphics;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// Contains extension methods for working with <see cref="Face"/>.
	/// </summary>
	public static partial class FaceExtensions
	{
		/// <summary>
		/// Gets the name of the PersonGroup formatted with its person count.
		/// </summary>
		/// <returns>The formatted group name.</returns>
		/// <param name="personGroup">Person group.</param>
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
			var largeFaceRect = face.CalculateLargeFaceRectangle (sourceImage);

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


		// Ratio to scale a detected face rectangle, the face rectangle scaled up looks more natural.
		const double FACE_RECT_SCALE_RATIO = 1.3;


		/// <summary>
		/// Resize face rectangle, for better view of the person.
		/// To make the rectangle larger, faceRectEnlargeRatio should be larger than 1, recommended value (and default) is 1.3.
		/// </summary>
		/// <returns>The resized face rectangle.</returns>
		/// <param name="face">The Face to calculate a new face renctagle for.</param>
		/// <param name="bitmap">the source Bitmap.</param>
		/// <param name="faceRectEnlargeRatio">Face rect enlarge ratio.</param>
		public static System.Drawing.RectangleF CalculateLargeFaceRectangle (this Model.Face face, Bitmap bitmap, double faceRectEnlargeRatio = FACE_RECT_SCALE_RATIO)
		{
			// Get the resized side length of the face rectangle
			double sideLength = face.FaceRectangle.Width * faceRectEnlargeRatio;
			sideLength = Math.Min (sideLength, bitmap.Width);
			sideLength = Math.Min (sideLength, bitmap.Height);

			// Make the left edge to left more.
			double left = face.FaceRectangle.Left - face.FaceRectangle.Width * (faceRectEnlargeRatio - 1.0) * 0.5;
			left = Math.Max (left, 0.0);
			left = Math.Min (left, bitmap.Width - sideLength);

			// Make the top edge to top more.
			double top = face.FaceRectangle.Top - face.FaceRectangle.Height * (faceRectEnlargeRatio - 1.0) * 0.5;
			top = Math.Max (top, 0.0);
			top = Math.Min (top, bitmap.Height - sideLength);

			// Shift the top edge to top more, for better view for human
			double shiftTop = faceRectEnlargeRatio - 1.0;
			shiftTop = Math.Max (shiftTop, 0.0);
			shiftTop = Math.Min (shiftTop, 1.0);
			top -= 0.15 * shiftTop * face.FaceRectangle.Height;
			top = Math.Max (top, 0.0);

			return new System.Drawing.RectangleF
			{
				X = (int) left,
				Y = (int) top,
				Width = (int) sideLength,
				Height = (int) sideLength
			};
		}


		/// <summary>
		/// Draw detected face rectangles in the original image. And return the image drawn.
		/// If drawLandmarks is set to be true, draw the five main landmarks of each face.
		/// </summary>
		/// <returns>A new bitmap with face rectangles drawn.  Note: original bitmap will remain as is and is not disposed/released.</returns>
		/// <param name="originalBitmap">Original bitmap.</param>
		/// <param name="faces">Faces.</param>
		/// <param name="drawLandmarks">If set to <c>true</c> draw landmarks.</param>
		/// <param name="faceRectEnlargeRatio">A scale factor that dicatates how much the face rectangles should be enlarged, if at all.</param>
		public static Bitmap DrawFaceRectangles (this Bitmap originalBitmap, IEnumerable<Model.Face> faces, bool drawLandmarks, double faceRectEnlargeRatio = FACE_RECT_SCALE_RATIO)
		{
			var bitmap = originalBitmap.Copy (Bitmap.Config.Argb8888, true);

			using (var canvas = new Canvas (bitmap))
			using (var paint = new Paint ())
			{
				paint.AntiAlias = true;
				paint.Color = Color.Green;
				paint.SetStyle (Paint.Style.Stroke);

				int stokeWidth = Math.Max (originalBitmap.Width, originalBitmap.Height) / 100;

				if (stokeWidth == 0)
				{
					stokeWidth = 1;
				}

				paint.StrokeWidth = stokeWidth;

				if (faces != null)
				{
					foreach (var face in faces)
					{
						var rect = face.CalculateLargeFaceRectangle (originalBitmap, faceRectEnlargeRatio);

						canvas.DrawRect (
							rect.Left,
							rect.Top,
							rect.Left + rect.Width,
							rect.Top + rect.Height,
							paint);

						if (drawLandmarks)
						{
							int radius = (int) rect.Width / 30;

							if (radius == 0)
							{
								radius = 1;
							}

							paint.SetStyle (Paint.Style.Fill);
							paint.StrokeWidth = radius;

							canvas.DrawCircle (
								face.Landmarks.PupilLeft.X,
								face.Landmarks.PupilLeft.Y,
								radius,
								paint);

							canvas.DrawCircle (
								face.Landmarks.PupilRight.X,
								face.Landmarks.PupilRight.Y,
								radius,
								paint);

							canvas.DrawCircle (
								face.Landmarks.NoseTip.X,
								face.Landmarks.NoseTip.Y,
								radius,
								paint);

							canvas.DrawCircle (
								face.Landmarks.MouthLeft.X,
								face.Landmarks.MouthLeft.Y,
								radius,
								paint);

							canvas.DrawCircle (
								face.Landmarks.MouthRight.X,
								face.Landmarks.MouthRight.Y,
								radius,
								paint);

							paint.SetStyle (Paint.Style.Stroke);
							paint.StrokeWidth = stokeWidth;
						}
					}
				}
			}

			return bitmap;
		}
	}
}