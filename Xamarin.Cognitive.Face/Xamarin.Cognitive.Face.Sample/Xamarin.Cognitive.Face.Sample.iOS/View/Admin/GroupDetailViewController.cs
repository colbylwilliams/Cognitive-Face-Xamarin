using System;
using Foundation;
using System.Threading.Tasks;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupDetailViewController : BaseViewController
	{
		internal static class Segues
		{
			public const string Embed = "Embed";
			public const string PersonDetail = "PersonDetail";
		}

		public PersonGroup Group => FaceState.Current.CurrentGroup;

		GroupPersonCollectionViewController GroupPersonCVC => ChildViewControllers [0] as GroupPersonCollectionViewController;

		public GroupDetailViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (Group != null)
			{
				GroupName.Text = Group.Name;

				checkTrainingStatus ().Forget ();
			}
		}


		partial void SaveAction (NSObject sender)
		{
			if (GroupName.Text.Length == 0)
			{
				this.ShowSimpleAlert ("Please input the group name");
				return;
			}

			if (Group == null)
			{
				createNewGroup ().Forget ();
			}
			else
			{
				updateGroup ().Forget ();
			}
		}


		partial void AddAction (NSObject sender)
		{
			if (GroupName.Text.Length == 0)
			{
				this.ShowSimpleAlert ("Please input the group name");
				return;
			}

			AddPerson ().Forget ();
		}


		async Task AddPerson ()
		{
			if (Group == null)
			{
				var createGroup = await this.ShowTwoOptionAlert ("Create Group?", "Do you want to create this new group?");

				if (!createGroup)
				{
					return;
				}

				await createNewGroup ();
			}

			if (Group != null) //just to make sure we succeeded in the case we created a new group above
			{
				PerformSegue (Segues.PersonDetail, this);
			}
		}


		async Task updateGroup ()
		{
			try
			{
				this.ShowHUD ("Saving & Training Group");

				await FaceClient.Shared.UpdatePersonGroup (Group, GroupName.Text);

				//_shouldExit = NO;
				await trainGroup ();

				this.ShowSimpleHUD ("Group saved");
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to update group.");
			}
		}


		async Task createNewGroup ()
		{
			try
			{
				this.ShowHUD ("Creating group");

				FaceState.Current.CurrentGroup = await FaceClient.Shared.CreatePersonGroup (GroupName.Text);

				this.ShowSimpleHUD ("Group created");

				GroupPersonCVC.CollectionView.ReloadData ();
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to create group.");
			}
		}


		async Task trainGroup ()
		{
			try
			{
				await FaceClient.Shared.TrainGroup (Group);

				//if (_shouldExit)
				//{
				//    this.NavigationController.PopViewController (true);
				//}
			}
			catch (Exception)
			{
				this.ShowSimpleHUD ("Failed in training group.");
			}
		}


		async Task checkTrainingStatus ()
		{
			var status = await FaceClient.Shared.GetGroupTrainingStatus (Group);

			switch (status.Status)
			{
				case "notstarted":
				case "failed":
					var result = await this.ShowTwoOptionAlert ("Training Status", "This group needs to be trained.  Train now?");

					if (result)
					{
						await trainGroup ();
					}

					break;
			}
		}
	}
}