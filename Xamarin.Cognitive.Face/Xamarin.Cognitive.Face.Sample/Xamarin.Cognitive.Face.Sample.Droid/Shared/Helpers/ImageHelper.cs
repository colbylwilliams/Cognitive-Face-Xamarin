using System;
using System.Collections.Generic;
using System.Drawing;
using Android.Graphics;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Droid.Contract;

namespace Xamarin.Cognitive.Face.Droid.Extensions
{
	public static class ImageHelper
	{
		/// <summary>
		/// Draw detected face rectangles in the original image. And return the image drawn.
		/// If drawLandmarks is set to be true, draw the five main landmarks of each face.
		/// </summary>
		/// <returns>The face rectangles on bitmap.</returns>
		/// <param name="originalBitmap">Original bitmap.</param>
		/// <param name="faces">Faces.</param>
		/// <param name="drawLandmarks">If set to <c>true</c> draw landmarks.</param>
		public static Bitmap DrawFaceRectanglesOnBitmap (Bitmap originalBitmap, IEnumerable<Model.Face> faces, bool drawLandmarks, double faceRectEnlargeRatio = FACE_RECT_SCALE_RATIO)
		{
			var bitmap = originalBitmap.Copy (Bitmap.Config.Argb8888, true);

			using (var canvas = new Canvas (bitmap))
			using (var paint = new Paint ())
			{
				paint.AntiAlias = true;
				paint.Color = global::Android.Graphics.Color.Green;
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
						var rect = face.FaceRectangle.CalculateLargeFaceRectangle (originalBitmap, faceRectEnlargeRatio);

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


		/// <summary>
		/// Highlight the selected face thumbnail in face list.
		/// </summary>
		/// <returns>The selected face thumbnail.</returns>
		/// <param name="originalBitmap">Original bitmap.</param>
		public static Bitmap HighlightSelectedFaceThumbnail (Bitmap originalBitmap)
		{
			var bitmap = originalBitmap.Copy (Bitmap.Config.Argb8888, true);

			using (var canvas = new Canvas (bitmap))
			using (var paint = new Paint ())
			{
				paint.AntiAlias = true;
				paint.Color = global::Android.Graphics.Color.ParseColor ("#3399FF");
				paint.SetStyle (Paint.Style.Stroke);

				int stokeWidth = Math.Max (originalBitmap.Width, originalBitmap.Height) / 10;

				if (stokeWidth == 0)
				{
					stokeWidth = 1;
				}

				paint.StrokeWidth = stokeWidth;

				canvas.DrawRect (
					0,
					0,
					bitmap.Width,
					bitmap.Height,
					paint);

				return bitmap;
			}
		}


		// Ratio to scale a detected face rectangle, the face rectangle scaled up looks more natural.
		const double FACE_RECT_SCALE_RATIO = 1.3;


		/// <summary>
		/// Resize face rectangle, for better view for human.
		/// To make the rectangle larger, faceRectEnlargeRatio should be larger than 1, recommend 1.3
		/// </summary>
		/// <returns>The face rectangle.</returns>
		/// <param name="faceRectangle">Face rectangle.</param>
		/// <param name="bitmap">the source Bitmap.</param>
		/// <param name="faceRectEnlargeRatio">Face rect enlarge ratio.</param>
		public static RectangleF CalculateLargeFaceRectangle (this RectangleF faceRectangle, Bitmap bitmap, double faceRectEnlargeRatio = FACE_RECT_SCALE_RATIO)
		{
			// Get the resized side length of the face rectangle
			double sideLength = faceRectangle.Width * faceRectEnlargeRatio;
			sideLength = Math.Min (sideLength, bitmap.Width);
			sideLength = Math.Min (sideLength, bitmap.Height);

			// Make the left edge to left more.
			double left = faceRectangle.Left
					- faceRectangle.Width * (faceRectEnlargeRatio - 1.0) * 0.5;
			left = Math.Max (left, 0.0);
			left = Math.Min (left, bitmap.Width - sideLength);

			// Make the top edge to top more.
			double top = faceRectangle.Top
					- faceRectangle.Height * (faceRectEnlargeRatio - 1.0) * 0.5;
			top = Math.Max (top, 0.0);
			top = Math.Min (top, bitmap.Height - sideLength);

			// Shift the top edge to top more, for better view for human
			double shiftTop = faceRectEnlargeRatio - 1.0;
			shiftTop = Math.Max (shiftTop, 0.0);
			shiftTop = Math.Min (shiftTop, 1.0);
			top -= 0.15 * shiftTop * faceRectangle.Height;
			top = Math.Max (top, 0.0);

			return new RectangleF
			{
				X = (int) left,
				Y = (int) top,
				Width = (int) sideLength,
				Height = (int) sideLength
			};
		}


		public static List<Bitmap> GenerateThumbnails (this List<Model.Face> faces, Bitmap photo)
		{
			var faceThumbnails = new List<Bitmap> ();

			if (faces != null)
			{
				foreach (var face in faces)
				{
					try
					{
						var largeFaceRect = face.FaceRectangle.CalculateLargeFaceRectangle (photo);

						faceThumbnails.Add (photo.Crop (largeFaceRect));
					}
					catch (Exception ex)
					{
						Log.Error (ex);
					}
				}
			}

			return faceThumbnails;
		}


		#region Legacy


		// Resize face rectangle, for better view for human
		// To make the rectangle larger, faceRectEnlargeRatio should be larger than 1, recommend 1.3
		static FaceRectangle CalculateFaceRectangle (
						Bitmap bitmap, FaceRectangle faceRectangle, double faceRectEnlargeRatio)
		{
			// Get the resized side length of the face rectangle
			double sideLength = faceRectangle.Width * faceRectEnlargeRatio;
			sideLength = Math.Min (sideLength, bitmap.Width);
			sideLength = Math.Min (sideLength, bitmap.Height);

			// Make the left edge to left more.
			double left = faceRectangle.Left
					- faceRectangle.Width * (faceRectEnlargeRatio - 1.0) * 0.5;
			left = Math.Max (left, 0.0);
			left = Math.Min (left, bitmap.Width - sideLength);

			// Make the top edge to top more.
			double top = faceRectangle.Top
					- faceRectangle.Height * (faceRectEnlargeRatio - 1.0) * 0.5;
			top = Math.Max (top, 0.0);
			top = Math.Min (top, bitmap.Height - sideLength);

			// Shift the top edge to top more, for better view for human
			double shiftTop = faceRectEnlargeRatio - 1.0;
			shiftTop = Math.Max (shiftTop, 0.0);
			shiftTop = Math.Min (shiftTop, 1.0);
			top -= 0.15 * shiftTop * faceRectangle.Height;
			top = Math.Max (top, 0.0);

			// Set the result.
			FaceRectangle result = new FaceRectangle ();
			result.Left = (int) left;
			result.Top = (int) top;
			result.Width = (int) sideLength;
			result.Height = (int) sideLength;

			return result;
		}


		// Crop the face thumbnail out from the original image.
		// For better view for human, face rectangles are resized to the rate faceRectEnlargeRatio.
		public static Bitmap GenerateFaceThumbnail (Bitmap originalBitmap, FaceRectangle faceRectangle)
		{
			FaceRectangle faceRect = CalculateFaceRectangle (originalBitmap, faceRectangle, 1.3);

			return Bitmap.CreateBitmap (originalBitmap, faceRect.Left, faceRect.Top, faceRect.Width, faceRect.Height);
		}


		#endregion
	}
}