using System;
using System.IO;
using CoreGraphics;
using UIKit;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// Contains extension methods for working with media: images, etc.
	/// </summary>
	public static class MediaExtensions
	{
		/// <summary>
		/// Gets the given UIImage as a JPEG Stream.
		/// </summary>
		/// <returns>A Stream with the image data.</returns>
		/// <param name="image">The UIImage.</param>
		public static Stream AsJpegStream (this UIImage image)
		{
			//will NSAutoreleasePool kill the intermediary NSData once the image and stream go away??
			var data = image.AsJPEG ();

			return data.AsStream ();
		}


		/// <summary>
		/// Crops the specified image using the rectangle.
		/// </summary>
		/// <returns>The cropped image.</returns>
		/// <param name="image">Image.</param>
		/// <param name="rect">Rect.</param>
		/// <remarks>The original image is not disposed of or released in any way.</remarks>
		public static UIImage Crop (this UIImage image, CGRect rect)
		{
			rect = new CGRect (rect.X * image.CurrentScale,
							   rect.Y * image.CurrentScale,
							   rect.Width * image.CurrentScale,
							   rect.Height * image.CurrentScale);

			using (CGImage cr = image.CGImage.WithImageInRect (rect))
			{
				var cropped = UIImage.FromImage (cr, image.CurrentScale, image.Orientation);

				return cropped;
			}
		}


		/// <summary>
		/// Saves the image in the JPEG format at the given path.
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="path">Path.</param>
		public static void SaveAsJpeg (this UIImage image, string path)
		{
			using (var data = image.AsJPEG ())
			{
				data.Save (path, true);
			}
		}


		/// <summary>
		/// Ensures that the UIImage is oriented in the UIImageOrientation.Up position.
		/// </summary>
		/// <returns>The image, oriented in the UIImageOrientation.Up position.</returns>
		/// <param name="image">The Image to validate orientation for.</param>
		public static UIImage FixOrientation (this UIImage image)
		{
			if (image.Orientation == UIImageOrientation.Up)
			{
				return image;
			}

			var transform = CGAffineTransform.MakeIdentity ();

			switch (image.Orientation)
			{
				case UIImageOrientation.Down:
				case UIImageOrientation.DownMirrored:
					transform = CGAffineTransform.Translate (transform, image.Size.Width, image.Size.Height);
					transform = CGAffineTransform.Rotate (transform, (float) Math.PI);
					break;
				case UIImageOrientation.Left:
				case UIImageOrientation.LeftMirrored:
					transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
					transform = CGAffineTransform.Rotate (transform, (float) Math.PI / 2);
					break;
				case UIImageOrientation.Right:
				case UIImageOrientation.RightMirrored:
					transform = CGAffineTransform.Translate (transform, 0, image.Size.Height);
					transform = CGAffineTransform.Rotate (transform, -(float) Math.PI / 2);
					break;
			}

			switch (image.Orientation)
			{
				case UIImageOrientation.UpMirrored:
				case UIImageOrientation.DownMirrored:
					transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
					transform = CGAffineTransform.Scale (transform, -1, 1);
					break;
				case UIImageOrientation.LeftMirrored:
				case UIImageOrientation.RightMirrored:
					transform = CGAffineTransform.Translate (transform, image.Size.Height, 0);
					transform = CGAffineTransform.Scale (transform, -1, 1);
					break;
			}

			using (var cgImg = image.CGImage)
			using (var ctx = new CGBitmapContext (null, (nint) image.Size.Width, (nint) image.Size.Height, cgImg.BitsPerComponent, 0, cgImg.ColorSpace, cgImg.BitmapInfo))
			{
				ctx.ConcatCTM (transform);

				switch (image.Orientation)
				{
					case UIImageOrientation.Left:
					case UIImageOrientation.LeftMirrored:
					case UIImageOrientation.Right:
					case UIImageOrientation.RightMirrored:
						ctx.DrawImage (new CGRect (0, 0, image.Size.Height, image.Size.Width), cgImg);
						break;
					default:
						ctx.DrawImage (new CGRect (0, 0, image.Size.Width, image.Size.Height), cgImg);
						break;
				}

				using (var newCgImg = ctx.ToImage ())
				{
					return UIImage.FromImage (newCgImg);
				}
			}
		}
	}
}