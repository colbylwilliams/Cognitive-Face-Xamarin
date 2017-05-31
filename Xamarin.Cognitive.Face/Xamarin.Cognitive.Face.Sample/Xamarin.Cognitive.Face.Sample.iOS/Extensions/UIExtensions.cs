using System;
using System.Threading.Tasks;
using Foundation;
using MBProgressHUD;
using NomadCode.UIExtensions;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public static class UIExtensions
	{
		static MTMBProgressHUD currentHud;

		public static void ShowHUD (this UIViewController vc, string message)
		{
			vc.HideHUD ();

			var hud = new MTMBProgressHUD (vc.NavigationController.View)
			{
				LabelText = message,
				RemoveFromSuperViewOnHide = true
			};

			vc.NavigationController.View.AddSubview (hud);
			hud.Show (true);

			currentHud = hud;
		}


		public static UIViewController HideHUD (this UIViewController vc)
		{
			if (currentHud != null)
			{
				currentHud.Hide (true);
				currentHud.RemoveFromSuperview ();
				currentHud = null;
			}

			return vc;
		}


		public static void ShowSimpleHUD (this UIViewController vc, string message)
		{
			vc.HideHUD ();

			var hud = new MTMBProgressHUD (vc.NavigationController.View)
			{
				LabelText = message,
				Mode = MBProgressHUDMode.Text,
				RemoveFromSuperViewOnHide = true
			};

			vc.NavigationController.View.AddSubview (hud);
			hud.Show (true);
			hud.Hide (true, 1.5);
		}


		public async static Task<UIImage> ShowImageSelectionDialog (this UIViewController vc)
		{
			var result = await vc.ShowActionSheet ("Select Image", "How would you like to choose an image?", "Select from album", "Take a photo");

			switch (result)
			{
				case "Select from album":
					return await vc.ShowPhotoPicker ();
				case "Take a photo":
					return await vc.ShowCameraPicker ();
				default:
					return null;
			}
		}
	}
}