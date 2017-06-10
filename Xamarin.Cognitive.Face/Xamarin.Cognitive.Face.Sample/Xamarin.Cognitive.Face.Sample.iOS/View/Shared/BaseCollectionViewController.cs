using System;
using Foundation;
using UIKit;
using Xamarin.Cognitive.Face.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public abstract class BaseCollectionViewController : UICollectionViewController
	{
		protected bool IsInitialLoad { get; private set; } = true;

		public BaseCollectionViewController (IntPtr handle) : base (handle)
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


		public override void ViewWillDisappear (bool animated)
		{
			//remove actions for any remaining visible cells
			foreach (var cell in CollectionView.VisibleCells)
			{
				if (cell is ISupportGestureAction)
				{
					((ISupportGestureAction) cell).DetachAction ();
				}
			}

			base.ViewWillDisappear (animated);
		}


		protected virtual Action<NSObject> GetGestureActionForCell (UICollectionViewCell cell)
		{
			return null;
		}


		public override void WillDisplayCell (UICollectionView collectionView, UICollectionViewCell cell, Foundation.NSIndexPath indexPath)
		{
			//if this cell is ISupportGestureAction, go ahead and hook those up here
			if (cell is ISupportGestureAction)
			{
				var action = GetGestureActionForCell (cell);

				if (action != null)
				{
					((ISupportGestureAction) cell).AttachAction (action);
				}
				else
				{
					throw new Exception ("Must return a valid action from GetGestureActionForCell when using ISupportGestureAction cells");
				}
			}
		}


		public override void CellDisplayingEnded (UICollectionView collectionView, UICollectionViewCell cell, Foundation.NSIndexPath indexPath)
		{
			//if this cell is ISupportGestureAction, go ahead and UNhook those here
			if (cell is ISupportGestureAction)
			{
				((ISupportGestureAction) cell).DetachAction ();
			}
		}
	}
}