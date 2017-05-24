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
	[Register ("VerificationViewController")]
	partial class VerificationViewController
	{
		[Outlet]
		UIKit.UIButton ChooseButton2 { get; set; }

		[Outlet]
		UIKit.UIImageView PersonImageView { get; set; }

		[Outlet]
		UIKit.UILabel PersonNameLabel { get; set; }

		[Outlet]
		UIKit.UIView PersonView { get; set; }

		[Outlet]
		UIKit.UIButton VerifyButton { get; set; }

		[Action ("ChooseImage1Action:")]
		partial void ChooseImage1Action (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (VerifyButton != null) {
				VerifyButton.Dispose ();
				VerifyButton = null;
			}

			if (ChooseButton2 != null) {
				ChooseButton2.Dispose ();
				ChooseButton2 = null;
			}

			if (PersonView != null) {
				PersonView.Dispose ();
				PersonView = null;
			}

			if (PersonNameLabel != null) {
				PersonNameLabel.Dispose ();
				PersonNameLabel = null;
			}

			if (PersonImageView != null) {
				PersonImageView.Dispose ();
				PersonImageView = null;
			}
		}
	}
}
