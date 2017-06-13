using System;
using Foundation;
using System.Threading.Tasks;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Model;

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

				if (IsInitialLoad)
				{
					CheckTrainingStatus ().Forget ();
				}
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
				CreateNewGroup ().Forget ();
			}
			else
			{
				UpdateGroup ().Forget ();
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

				await CreateNewGroup ();
			}

			if (Group != null) //just to make sure we succeeded in the case we created a new group above
			{
				FaceState.Current.CurrentPerson = null;

				PerformSegue (Segues.PersonDetail, this);
			}
		}


		async Task UpdateGroup ()
		{
			try
			{
				this.ShowHUD ("Saving & Training Group");

				await FaceClient.Shared.UpdatePersonGroup (Group, GroupName.Text);

				//_shouldExit = NO;
				await TrainGroup ();

				this.ShowSimpleHUD ("Group saved");
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to update group.");
			}
		}


		async Task CreateNewGroup ()
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


		async Task TrainGroup ()
		{
			try
			{
				await FaceClient.Shared.TrainPersonGroup (Group);

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


		async Task CheckTrainingStatus ()
		{
			try
			{
				var status = await FaceClient.Shared.GetGroupTrainingStatus (Group);

				switch (status.Status)
				{
					case TrainingStatus.TrainingStatusType.NotStarted:
					case TrainingStatus.TrainingStatusType.Failed:
						var result = await this.ShowTwoOptionAlert ("Training Status", "This group needs to be trained.  Train now?");

						if (result)
						{
							await TrainGroup ();
						}

						break;
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.ShowSimpleHUD ("Failed to get group training status.");
			}
		}
	}
}