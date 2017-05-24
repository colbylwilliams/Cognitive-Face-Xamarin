// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	[Register ("IdentifyResultTableViewCell")]
	partial class IdentifyResultTableViewCell
	{
		[Outlet]
		UIKit.UILabel ConfidenceLabel { get; set; }

		[Outlet]
		UIKit.UIImageView FaceImageView { get; set; }

		[Outlet]
		UIKit.UILabel PersonNameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FaceImageView != null) {
				FaceImageView.Dispose ();
				FaceImageView = null;
			}

			if (PersonNameLabel != null) {
				PersonNameLabel.Dispose ();
				PersonNameLabel = null;
			}

			if (ConfidenceLabel != null) {
				ConfidenceLabel.Dispose ();
				ConfidenceLabel = null;
			}
		}
	}
}
