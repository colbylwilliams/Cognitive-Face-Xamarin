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
		/// Crops the specified image using the rectangle.
		/// </summary>
		/// <returns>The cropped image.</returns>
		/// <param name="image">Image.</param>
		/// <param name="rect">Rect.</param>
		/// <remarks>The image uses is not disposed of or released in any way.</remarks>
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