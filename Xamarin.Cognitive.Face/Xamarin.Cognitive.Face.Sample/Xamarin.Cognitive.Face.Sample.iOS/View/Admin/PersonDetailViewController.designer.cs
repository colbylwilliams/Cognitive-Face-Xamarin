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
	[Register ("PersonDetailViewController")]
	partial class PersonDetailViewController
	{
		[Outlet]
		UIKit.UITextField PersonName { get; set; }

		[Action ("AddFaceAction:")]
		partial void AddFaceAction (Foundation.NSObject sender);

		[Action ("SaveAction:")]
		partial void SaveAction (Foundation.NSObject sender);

		[Action ("VerifyAction:")]
		partial void VerifyAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (PersonName != null) {
				PersonName.Dispose ();
				PersonName = null;
			}
		}
	}
}
