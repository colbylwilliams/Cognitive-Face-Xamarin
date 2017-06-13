using System;
using UIKit;
using CoreGraphics;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FaceCVC : UICollectionViewCell
	{
		Model.Face face;

		public override bool Highlighted
		{
			get
			{
				return base.Highlighted;
			}
			set
			{
				base.Highlighted = value;
				SetNeedsDisplay ();
			}
		}


		public FaceCVC (IntPtr handle) : base (handle)
		{
		}


		public override void Draw (CGRect rect)
		{
			base.Draw (rect);

			if (Highlighted)
			{
				CGContext context = UIGraphics.GetCurrentContext ();

				context.SetFillColor (1, 0, 0, 1);
				context.FillRect (Bounds);
			}
		}


		public void SetFaceImage (Model.Face face, UIImage image)
		{
			this.face = face;
			FaceImageView.Image = image;
		}
	}
}