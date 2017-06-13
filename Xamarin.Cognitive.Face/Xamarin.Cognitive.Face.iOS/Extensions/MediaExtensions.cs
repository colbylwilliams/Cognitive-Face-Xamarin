using CoreGraphics;
using UIKit;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static class MediaExtensions
	{
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


		public static void SaveAsJpeg (this UIImage image, string path)
		{
			using (var data = image.AsJPEG ())
			{
				data.Save (path, true);
			}
		}
	}
}