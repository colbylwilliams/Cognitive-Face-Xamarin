using System;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupPersonCollectionViewController : ItemsPerRowCollectionViewController
	{
		public PersonGroup Group => FaceState.Current.CurrentGroup;


		public GroupPersonCollectionViewController (IntPtr handle) : base (handle)
		{
			//make our cells longer than they are wide - to account for the text we'll be adding
			HeightMultiplier = 4d / 3d;
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (IsInitialLoad)
			{
				if (Group != null)
				{
					loadPeople ().Forget ();
					return;
				}
			}

			CollectionView.ReloadData ();
		}


		async Task loadPeople ()
		{
			try
			{
				this.ShowHUD ("Loading group");

				if (Group.People?.Count == 0)
				{
					await FaceClient.Shared.GetPeopleForGroup (Group);
				}

				foreach (var person in Group.People)
				{
					if (person.Faces?.Count == 0)
					{
						await FaceClient.Shared.GetFacesForPerson (person, Group);
					}
				}

				CollectionView.ReloadData ();

				this.HideHUD ();
			}
			catch (Exception ex)
			{
				Log.Error ($"Error getting people for group (FaceClient.Shared.GetPeopleForGroup) :: {ex.Message}");

				this.ShowSimpleHUD ("Error retrieving people for group");
			}
		}


		public override nint NumberOfSections (UICollectionView collectionView) => Group?.People?.Count ?? 0;


		public override UICollectionReusableView GetViewForSupplementaryElement (UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
		{
			var person = Group.People [indexPath.Section];
			var header = collectionView.Dequeue<SimpleCVHeader> (UICollectionElementKindSection.Header, indexPath);

			header.SetTitle (person.Name);

			return header;
		}


		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			var faces = Group?.People? [(int) section]?.Faces;

			if (faces != null)
			{
				return faces.Count == 0 ? 1 : faces.Count; //always return 1 so we can draw a dummy face cell and allow deletion, etc.
			}

			return 0;
		}


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.Dequeue<GroupPersonCVC> (indexPath);

			cell.SetPerson (Group.People [indexPath.Section], indexPath.Section, indexPath.Row);

			return cell;
		}


		protected override Action<NSObject> GetGestureActionForCell (UICollectionViewCell cell)
		{
			return longPressAction;
		}


		async void longPressAction (NSObject nsObj)
		{
			var gestureRecognizer = (UIGestureRecognizer) nsObj;

			if (gestureRecognizer.State == UIGestureRecognizerState.Began)
			{
				try
				{
					var personIndex = gestureRecognizer.View.Tag;
					var person = Group.People [(int) personIndex];

					var result = await this.ShowActionSheet ($"Do you want to remove all of {person.Name}'s faces?", string.Empty, "Yes");

					if (result == "Yes")
					{
						this.ShowHUD ($"Deleting {person.Name}");

						await FaceClient.Shared.DeletePerson (person, Group);

						this.ShowSimpleHUD ($"{person.Name} deleted");

						CollectionView.ReloadData ();
					}
				}
				catch (Exception)
				{
					this.HideHUD ().ShowSimpleAlert ("Failed to delete person.");
				}
			}
		}


		public async override void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			FaceState.Current.CurrentPerson = Group.People [indexPath.Section];

			if (!FaceState.Current.Verification.NeedsPerson)
			{
				ParentViewController.PerformSegue (GroupDetailViewController.Segues.PersonDetail, this);
			}
			else
			{
				var choice = await this.ShowActionSheet ("Please choose", "What would you like to do with the selected person?", "Use for verification", "Edit");

				switch (choice)
				{
					case "Use for verification":
						var face = FaceState.Current.CurrentPerson.Faces [indexPath.Row];
						FaceState.Current.Verification.Face = face;
						FaceState.Current.Verification.Person = FaceState.Current.CurrentPerson;
						FaceState.Current.Verification.Group = Group;

						this.PopTo<VerificationViewController> ();

						break;
					case "Edit":
						ParentViewController.PerformSegue (GroupDetailViewController.Segues.PersonDetail, this);
						break;
					default:
						return;
				}
			}
		}
	}
}