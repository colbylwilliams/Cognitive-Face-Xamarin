using System;
using UIKit;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public class BaseViewController : UIViewController
	{
		protected bool IsInitialLoad { get; private set; } = true;

		public BaseViewController (IntPtr handle) : base (handle)
		{
		}


		protected override void Dispose (bool disposing)
		{
			Log.Info ($"Disposing {GetType ()}");

			base.Dispose (disposing);
		}


		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			IsInitialLoad = false;
		}
	}
}