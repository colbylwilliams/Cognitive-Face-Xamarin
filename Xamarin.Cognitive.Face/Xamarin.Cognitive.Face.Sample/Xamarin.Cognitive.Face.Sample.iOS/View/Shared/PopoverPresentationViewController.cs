using System;
using Foundation;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public abstract class PopoverPresentationViewController : BaseViewController, IUIPopoverPresentationControllerDelegate
	{
		UIBarButtonItem doneItem;

		protected PopoverPresentationViewController (IntPtr handle) : base (handle)
		{
		}


		[Export ("adaptivePresentationStyleForPresentationController:")]
		public UIModalPresentationStyle GetAdaptivePresentationStyle (UIPresentationController forPresentationController)
		{
			return UIModalPresentationStyle.FullScreen;
		}


		[Export ("presentationController:viewControllerForAdaptivePresentationStyle:")]
		public UIViewController GetViewControllerForAdaptivePresentation (UIPresentationController controller, UIModalPresentationStyle style)
		{
			var navController = new UINavigationController (controller.PresentedViewController);

			if (navController != null)
			{
				var closeText = GetPopoverCloseText (navController.TopViewController);
				doneItem = new UIBarButtonItem (closeText, UIBarButtonItemStyle.Done, null);
				doneItem.Clicked += DoneTapped;
				navController.TopViewController.NavigationItem.RightBarButtonItem = doneItem;
			}

			return navController;
		}


		protected virtual string GetPopoverCloseText (UIViewController presentedViewController)
		{
			return "Cancel";
		}


		public void DoneTapped (object sender, EventArgs e)
		{
			doneItem.Clicked -= DoneTapped;
			doneItem.Action = null;
			doneItem.Dispose ();
			doneItem = null;


			//noticing some memory being hung onto here, so explicitly disposing
			var navController = PresentedViewController as UINavigationController;
			var presentationController = navController.PresentationController;
			var presentedController = presentationController.PresentedViewController;



			DismissViewController (true, () =>
			{
				navController.TopViewController.NavigationItem.RightBarButtonItem = null;

				//presentedController.PopoverPresentationController.Delegate = null;
				presentedController.PopoverPresentationController.Dispose ();

				presentedController.Dispose ();
				presentedController = null;

				presentationController.Delegate = null;
				presentationController.WeakDelegate = null;
				((UIPopoverPresentationController)presentationController).SourceView = new UIButton ();

				presentationController.Dispose ();
				presentationController = null;



				navController.SetViewControllers (null, false);
				navController.Dispose ();
				navController = null;
			});
		}
	}
}