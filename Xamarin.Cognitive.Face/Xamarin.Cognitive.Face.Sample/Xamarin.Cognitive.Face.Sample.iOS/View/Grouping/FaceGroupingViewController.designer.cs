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
	[Register ("FaceGroupingViewController")]
	partial class FaceGroupingViewController
	{
		[Outlet]
		UIKit.UIButton GroupButton { get; set; }

		[Outlet]
		UIKit.UILabel TotalFacesLabel { get; set; }

		[Action ("AddFaceAction:")]
		partial void AddFaceAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (TotalFacesLabel != null) {
				TotalFacesLabel.Dispose ();
				TotalFacesLabel = null;
			}

			if (GroupButton != null) {
				GroupButton.Dispose ();
				GroupButton = null;
			}
		}
	}
}
