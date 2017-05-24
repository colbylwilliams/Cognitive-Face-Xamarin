using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	/// <summary>
	/// This UICollectionViewController will display <see cref="CellsAcross"/> number of cells in a row across the width of the UICollectionView.
	/// </summary>
	public class ItemsPerRowCollectionViewController : BaseCollectionViewController, IUICollectionViewDelegateFlowLayout
	{
		protected int CellsAcross { get; set; } = 3;
		protected int MarginWidth { get; set; } = 10;
		protected int ItemSpacing { get; set; } = 10;
		protected double HeightMultiplier { get; set; } = 1;

		public ItemsPerRowCollectionViewController (IntPtr handle) : base (handle)
		{
		}


		[Export ("collectionView:layout:sizeForItemAtIndexPath:")]
		public CGSize GetSizeForItem (UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		{
			return new CGSize ((CollectionView.Frame.Width - MarginWidth) / CellsAcross - ItemSpacing,
							   ((CollectionView.Frame.Width - MarginWidth) / CellsAcross - ItemSpacing) * HeightMultiplier);
		}


		[Export ("collectionView:layout:minimumLineSpacingForSectionAtIndex:")]
		public nfloat GetMinimumLineSpacingForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return 10;
		}


		[Export ("collectionView:layout:minimumInteritemSpacingForSectionAtIndex:")]
		public nfloat GetMinimumInteritemSpacingForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return ItemSpacing;
		}


		[Export ("collectionView:layout:insetForSectionAtIndex:")]
		public UIEdgeInsets GetInsetForSection (UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return new UIEdgeInsets (MarginWidth, MarginWidth, MarginWidth, MarginWidth);
		}
	}
}