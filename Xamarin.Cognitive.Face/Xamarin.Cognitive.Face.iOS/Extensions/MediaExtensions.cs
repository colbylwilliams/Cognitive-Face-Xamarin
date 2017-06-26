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
	}
}