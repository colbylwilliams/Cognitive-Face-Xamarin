using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Provider;
using Java.Lang;
using Xamarin.Cognitive.Face.Droid.Contract;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	public class ImageHelper
	{
		// The maximum side length of the image to detect, to keep the size of image less than 4MB.
		// Resize the image if its side length is larger than the maximum.
		private static int IMAGE_MAX_SIDE_LENGTH = 1280;

		// Ratio to scale a detected face rectangle, the face rectangle scaled up looks more natural.
		private static double FACE_RECT_SCALE_RATIO = 1.3;

		// Decode image from imageUri, and resize according to the expectedMaxImageSideLength
		// If expectedMaxImageSideLength is
		//     (1) less than or equal to 0,
		//     (2) more than the actual max size length of the bitmap
		//     then return the original bitmap
		// Else, return the scaled bitmap
		public static Bitmap LoadSizeLimitedBitmapFromUri (Uri imageUri, ContentResolver contentResolver)
		{
			try
			{
				// Load the image into InputStream.
				System.IO.Stream imageInputStream = contentResolver.OpenInputStream (imageUri);

				// For saving memory, only decode the image meta and get the side length.
				BitmapFactory.Options options = new BitmapFactory.Options ();
				options.InJustDecodeBounds = true;
				Rect outPadding = new Rect ();
				BitmapFactory.DecodeStream (imageInputStream, outPadding, options);

				// Calculate shrink rate when loading the image into memory.
				int maxSideLength =
						options.OutWidth > options.OutHeight ? options.OutWidth : options.OutHeight;
				options.InSampleSize = 1;
				options.InSampleSize = calculateSampleSize (maxSideLength, IMAGE_MAX_SIDE_LENGTH);
				options.InJustDecodeBounds = false;
				if (imageInputStream != null)
				{
					imageInputStream.Close ();
				}

				// Load the bitmap and resize it to the expected size length
				imageInputStream = contentResolver.OpenInputStream (imageUri);
				Bitmap bitmap = BitmapFactory.DecodeStream (imageInputStream, outPadding, options);
				maxSideLength = bitmap.Width > bitmap.Height
						? bitmap.Width : bitmap.Height;
				double ratio = IMAGE_MAX_SIDE_LENGTH / (double) maxSideLength;
				if (ratio < 1)
				{
					bitmap = Bitmap.CreateScaledBitmap (
							bitmap,
							(int) (bitmap.Width * ratio),
							(int) (bitmap.Height * ratio),
							false);
				}

				return RotateBitmap (bitmap, GetImageRotationAngle (imageUri, contentResolver));
			}
			catch (Exception)
			{
				return null;
			}
		}

		// Draw detected face rectangles in the original image. And return the image drawn.
		// If drawLandmarks is set to be true, draw the five main landmarks of each face.
		public static Bitmap DrawFaceRectanglesOnBitmap (Bitmap originalBitmap, Face.Droid.Contract.Face [] faces, bool drawLandmarks)
		{
			Bitmap bitmap = originalBitmap.Copy (Bitmap.Config.Argb8888, true);
			Canvas canvas = new Canvas (bitmap);

			Paint paint = new Paint ();
			paint.AntiAlias = true;
			paint.SetStyle (Paint.Style.Stroke);
			paint.Color = Color.Green;
			int stokeWidth = Math.Max (originalBitmap.Width, originalBitmap.Height) / 100;
			if (stokeWidth == 0)
			{
				stokeWidth = 1;
			}
			paint.StrokeWidth = stokeWidth;

			if (faces != null)
			{
				foreach (Face.Droid.Contract.Face face in faces)
				{
					FaceRectangle faceRectangle = CalculateFaceRectangle (bitmap, face.FaceRectangle, FACE_RECT_SCALE_RATIO);

					canvas.DrawRect (
							faceRectangle.Left,
							faceRectangle.Top,
							faceRectangle.Left + faceRectangle.Width,
							faceRectangle.Top + faceRectangle.Height,
							paint);

					if (drawLandmarks)
					{
						int radius = face.FaceRectangle.Width / 30;
						if (radius == 0)
						{
							radius = 1;
						}
						paint.SetStyle (Paint.Style.Fill);
						paint.StrokeWidth = radius;

						canvas.DrawCircle (
								(float) face.FaceLandmarks.PupilLeft.X,
								(float) face.FaceLandmarks.PupilLeft.Y,
								radius,
								paint);

						canvas.DrawCircle (
								(float) face.FaceLandmarks.PupilRight.X,
								(float) face.FaceLandmarks.PupilRight.Y,
								radius,
								paint);

						canvas.DrawCircle (
								(float) face.FaceLandmarks.NoseTip.X,
								(float) face.FaceLandmarks.NoseTip.Y,
								radius,
								paint);

						canvas.DrawCircle (
								(float) face.FaceLandmarks.MouthLeft.X,
								(float) face.FaceLandmarks.MouthLeft.Y,
								radius,
								paint);

						canvas.DrawCircle (
								(float) face.FaceLandmarks.MouthRight.X,
								(float) face.FaceLandmarks.MouthRight.Y,
								radius,
								paint);

						paint.SetStyle (Paint.Style.Stroke);
						paint.StrokeWidth = stokeWidth;
					}
				}
			}

			return bitmap;
		}

		// Highlight the selected face thumbnail in face list.
		public static Bitmap HighlightSelectedFaceThumbnail (Bitmap originalBitmap)
		{
			Bitmap bitmap = originalBitmap.Copy (Bitmap.Config.Argb8888, true);
			Canvas canvas = new Canvas (bitmap);
			Paint paint = new Paint ();
			paint.AntiAlias = true;
			paint.SetStyle (Paint.Style.Stroke);
			paint.Color = Color.ParseColor ("#3399FF");
			int stokeWidth = Math.Max (originalBitmap.Width, originalBitmap.Height) / 10;
			if (stokeWidth == 0)
			{
				stokeWidth = 1;
			}
			//bitmap.getWidth;
			paint.StrokeWidth = stokeWidth;
			canvas.DrawRect (
					0,
					0,
					bitmap.Width,
					bitmap.Height,
					paint);

			return bitmap;
		}

		// Crop the face thumbnail out from the original image.
		// For better view for human, face rectangles are resized to the rate faceRectEnlargeRatio.
		public static Bitmap GenerateFaceThumbnail (Bitmap originalBitmap, FaceRectangle faceRectangle)
		{
			FaceRectangle faceRect = CalculateFaceRectangle (originalBitmap, faceRectangle, FACE_RECT_SCALE_RATIO);

			return Bitmap.CreateBitmap (originalBitmap, faceRect.Left, faceRect.Top, faceRect.Width, faceRect.Height);
		}

		// Return the number of times for the image to shrink when loading it into memory.
		// The SampleSize can only be a final value based on powers of 2.
		private static int calculateSampleSize (int maxSideLength, int expectedMaxImageSideLength)
		{
			int inSampleSize = 1;

			while (maxSideLength > 2 * expectedMaxImageSideLength)
			{
				maxSideLength /= 2;
				inSampleSize *= 2;
			}

			return inSampleSize;
		}

		// Get the rotation angle of the image taken.
		private static int GetImageRotationAngle (Uri imageUri, ContentResolver contentResolver)
		{
			int angle = 0;
			ICursor cursor = contentResolver.Query (imageUri, new [] { MediaStore.Images.ImageColumns.Orientation }, null, null, null);
			if (cursor != null)
			{
				if (cursor.Count == 1)
				{
					cursor.MoveToFirst ();
					angle = cursor.GetInt (0);
				}
				cursor.Close ();
			}
			else
			{
				ExifInterface exif = new ExifInterface (imageUri.Path);
				int orientation = exif.GetAttributeInt (ExifInterface.TagOrientation, (int) Orientation.Normal);

				switch (orientation)
				{
					case (int) Orientation.Rotate270:
						angle = 270;
						break;
					case (int) Orientation.Rotate180:
						angle = 180;
						break;
					case (int) Orientation.Rotate90:
						angle = 90;
						break;
					default:
						break;
				}
			}
			return angle;
		}

		// Rotate the original bitmap according to the given orientation angle
		private static Bitmap RotateBitmap (Bitmap bitmap, int angle)
		{
			// If the rotate angle is 0, then return the original image, else return the rotated image
			if (angle != 0)
			{
				Matrix matrix = new Matrix ();
				matrix.PostRotate (angle);
				return Bitmap.CreateBitmap (
						bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
			}
			else
			{
				return bitmap;
			}
		}

		// Resize face rectangle, for better view for human
		// To make the rectangle larger, faceRectEnlargeRatio should be larger than 1, recommend 1.3
		private static FaceRectangle CalculateFaceRectangle (
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
	}
}