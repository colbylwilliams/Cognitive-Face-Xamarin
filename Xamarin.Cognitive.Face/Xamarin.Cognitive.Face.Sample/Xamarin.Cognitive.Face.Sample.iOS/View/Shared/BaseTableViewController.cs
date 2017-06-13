using System;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public abstract class BaseTableViewController : UITableViewController
	{
		protected bool IsInitialLoad { get; private set; } = true;

		public BaseTableViewController (IntPtr handle) : base (handle)
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