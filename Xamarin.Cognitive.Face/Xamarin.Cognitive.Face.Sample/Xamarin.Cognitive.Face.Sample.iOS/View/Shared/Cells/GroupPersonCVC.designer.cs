// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
    [Register ("GroupPersonCVC")]
    partial class GroupPersonCVC
    {
        [Outlet]
        UIKit.UIImageView ImageView { get; set; }

        [Outlet]
        UIKit.UILabel TextView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ImageView != null)
            {
                ImageView.Dispose ();
                ImageView = null;
            }

            if (TextView != null)
            {
                TextView.Dispose ();
                TextView = null;
            }
        }
    }
}
