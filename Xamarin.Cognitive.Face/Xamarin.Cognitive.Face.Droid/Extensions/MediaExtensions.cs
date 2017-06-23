using System;
using System.Drawing;
using System.IO;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Media;
using Android.Provider;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// Contains extension methods for working with media: images, etc.
	/// </summary>
	public static class MediaExtensions
	{
		/// <summary>
		/// Gets the given Bitmap as a JPEG Stream and resets the stream position to 0.
		/// </summary>
		/// <returns>A Stream with the image data.</returns>
		/// <param name="bitmap">The Bitmap.</param>
		/// <param name="quality">The quality factor to use when compressing as a JPEG.</param>
		public static System.IO.Stream AsJpeg (this Bitmap bitmap, int quality = 100)
		{
			var stream = new MemoryStream ();

			if (!bitmap.Compress (Bitmap.CompressFormat.Jpeg, quality, stream))
			{
				stream?.Dispose ();

				throw new Exception ("Compression to JPEG failed");
			}

			stream.Position = 0;

			return stream;
		}


		/// <summary>
		/// Saves the image in the JPEG format at the given path.
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="path">Path.</param>
		/// <param name="quality">The quality factor to use when compressing as a JPEG.</param>
		public static void SaveAsJpeg (this Bitmap image, string path, int quality = 100)
		{
			using (var fs = new FileStream (path, FileMode.OpenOrCreate))
			{
				image.Compress (Bitmap.CompressFormat.Jpeg, quality, fs);
			}
		}


		/// <summary>
		/// Crops the specified image using the rectangle.
		/// </summary>
		/// <returns>The cropped image.</returns>
		/// <param name="image">Image.</param>
		/// <param name="rect">Rect.</param>
		/// <remarks>The image uses is not disposed of or released in any way.</remarks>
		public static Bitmap Crop (this Bitmap image, RectangleF rect)
		{
			return Bitmap.CreateBitmap (image, (int) rect.Left, (int) rect.Top, (int) rect.Width, (int) rect.Height);
		}


		/// <summary>
		/// Decode image from imageUri, and resize according to the expectedMaxImageSideLength
		/// If expectedMaxImageSideLength is
		///     (1) less than or equal to 0,
		///     (2) more than the actual max size length of the bitmap
		///     then return the original bitmap
		/// Else, return the scaled bitmap
		/// </summary>
		/// <returns>The size limited bitmap from URI.</returns>
		/// <param name="contentResolver">Content resolver.</param>
		/// <param name="imageUri">Image URI.</param>
		/// <param name="maxImageSideLength">The maximum side length of the image to detect, to keep the size of image less than 4MB.  Resize the image if its side length is larger than the maximum.</param>
		public static Bitmap LoadSizeLimitedBitmapFromUri (this ContentResolver contentResolver, global::Android.Net.Uri imageUri, int maxImageSideLength = 1280)
		{
			try
			{
				var outPadding = new Rect ();
				int maxSideLength = 0;

				// For saving memory, only decode the image meta and get the side length.
				var options = new BitmapFactory.Options
				{
					InJustDecodeBounds = true
				};

				using (var fileDescriptor = contentResolver.OpenFileDescriptor (imageUri, "r"))
				{
					using (BitmapFactory.DecodeFileDescriptor (fileDescriptor.FileDescriptor, outPadding, options))
					{
						// Calculate shrink rate when loading the image into memory.
						maxSideLength = options.OutWidth > options.OutHeight ? options.OutWidth : options.OutHeight;
						options.InSampleSize = 1;
						options.InSampleSize = calculateSampleSize (maxSideLength, maxImageSideLength);
						options.InJustDecodeBounds = false;
					}

					// Load the bitmap and resize it to the expected size length
					var bitmap = BitmapFactory.DecodeFileDescriptor (fileDescriptor.FileDescriptor, outPadding, options);

					maxSideLength = bitmap.Width > bitmap.Height ? bitmap.Width : bitmap.Height;
					double ratio = maxImageSideLength / (double) maxSideLength;

					if (ratio < 1)
					{
						var rotatedBitmap = Bitmap.CreateScaledBitmap (
							bitmap,
							(int) (bitmap.Width * ratio),
							(int) (bitmap.Height * ratio),
							false);

						if (rotatedBitmap != bitmap)
						{
							bitmap.Dispose ();
							bitmap = rotatedBitmap;
						}
					}

					var returnBitmap = RotateBitmap (bitmap, GetImageRotationAngle (imageUri, contentResolver));

					//kill this bitmap if rotate created a new one
					if (returnBitmap != bitmap)
					{
						bitmap.Dispose ();
					}

					return returnBitmap;
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				return null;
			}
		}


		/// <summary>
		/// Return the number of times for the image to shrink when loading it into memory.
		/// The SampleSize can only be a final value based on powers of 2.
		/// </summary>
		/// <returns>The sample size.</returns>
		/// <param name="maxSideLength">Max side length.</param>
		/// <param name="expectedMaxImageSideLength">Expected max image side length.</param>
		static int calculateSampleSize (int maxSideLength, int expectedMaxImageSideLength)
		{
			int inSampleSize = 1;

			while (maxSideLength > 2 * expectedMaxImageSideLength)
			{
				maxSideLength /= 2;
				inSampleSize *= 2;
			}

			return inSampleSize;
		}


		/// <summary>
		/// Get the rotation angle of the image taken.
		/// </summary>
		/// <returns>The image rotation angle.</returns>
		/// <param name="imageUri">Image URI.</param>
		/// <param name="contentResolver">Content resolver.</param>
		public static int GetImageRotationAngle (global::Android.Net.Uri imageUri, ContentResolver contentResolver)
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
				var exif = new ExifInterface (imageUri.Path);
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
				}
			}

			return angle;
		}


		/// <summary>
		/// Rotate the original bitmap according to the given orientation angle.
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="bitmap">Bitmap.</param>
		/// <param name="angle">Angle.</param>
		static Bitmap RotateBitmap (Bitmap bitmap, int angle)
		{
			// If the rotate angle is 0, then return the original image, else return the rotated image
			if (angle != 0)
			{
				var matrix = new Matrix ();
				matrix.PostRotate (angle);

				return Bitmap.CreateBitmap (bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
			}

			return bitmap;
		}


		/// <summary>
		/// Draws a highlight around the face thumbnail.
		/// </summary>
		/// <returns>The highlighted face thumbnail.  Note this is a new bitmap and the original is not disposed.</returns>
		/// <param name="thumbnail">Original bitmap.</param>
		/// <param name="colorHex">The hex code of the desired color to highlight with.</param>
		public static Bitmap AddHighlight (this Bitmap thumbnail, string colorHex = "#3399FF")
		{
			var bitmap = thumbnail.Copy (Bitmap.Config.Argb8888, true);

			using (var canvas = new Canvas (bitmap))
			using (var paint = new Paint ())
			{
				paint.AntiAlias = true;
				paint.Color = global::Android.Graphics.Color.ParseColor (colorHex);
				paint.SetStyle (Paint.Style.Stroke);

				int stokeWidth = Math.Max (thumbnail.Width, thumbnail.Height) / 10;

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
	}
}