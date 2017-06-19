using System;
using Foundation;
using UIKit;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupsViewController : BaseViewController
	{
		class Segues
		{
			public const string Embed = "Embed";
			public const string Detail = "GroupDetail";
		}

		GroupsTableViewController GroupsTableController => ChildViewControllers [0] as GroupsTableViewController;

		public GroupsViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			GroupsTableController.RestoreSelectionOnAppear = false;
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			GroupsTableController.GroupSelectionChanged += OnGroupSelectionChanged;
		}


		public override void ViewWillDisappear (bool animated)
		{
			GroupsTableController.GroupSelectionChanged -= OnGroupSelectionChanged;

			base.ViewWillDisappear (animated);
		}


		void OnGroupSelectionChanged (object sender, PersonGroup selection)
		{
			FaceState.Current.CurrentGroup = selection;

			PerformSegue (Segues.Detail, this);
		}


		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier == Segues.Embed && segue.DestinationViewController is GroupsTableViewController groupsTVC)
			{
				groupsTVC.AllowDelete = true;
			}
		}
	}
}