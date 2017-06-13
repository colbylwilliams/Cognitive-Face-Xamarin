using System;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class PersonFaceCollectionViewController : ItemsPerRowCollectionViewController
	{
		public PersonGroup Group => FaceState.Current.CurrentGroup;
		public Person Person => FaceState.Current.CurrentPerson;

		public PersonFaceCollectionViewController (IntPtr handle) : base (handle)
		{
			//make our cells longer than they are wide - to account for the text we'll be adding
			HeightMultiplier = 4d / 3d;
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (!IsInitialLoad)
			{
				CollectionView.ReloadData ();
			}
		}


		public override nint NumberOfSections (UICollectionView collectionView) => 1;


		public override nint GetItemsCount (UICollectionView collectionView, nint section) => Person?.Faces?.Count ?? 0;


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.Dequeue<GroupPersonCVC> (indexPath);

			cell.SetPerson (Person, indexPath.Row, indexPath.Row);

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
					var faceIndex = gestureRecognizer.View.Tag;

					var result = await this.ShowActionSheet ("Do you want to remove this face?", string.Empty, "Yes");

					if (result == "Yes")
					{
						var face = Person.Faces [(int) faceIndex];

						this.ShowHUD ("Deleting this face");

						await FaceClient.Shared.DeletePersonFace (Person, Group, face);

						this.ShowSimpleHUD ("Face deleted");

						CollectionView.ReloadData ();
					}
				}
				catch (Exception)
				{
					this.HideHUD ().ShowSimpleAlert ("Failed to delete person.");
				}
			}
		}
	}
}