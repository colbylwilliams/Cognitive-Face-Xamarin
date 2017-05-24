using System;
using System.Collections.Generic;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupingResultCollectionViewController : BaseCollectionViewController
	{
		List<FaceGroup> Results;

		public GroupingResultCollectionViewController (IntPtr handle) : base (handle)
		{
		}


		public void SetFaceGroupResults (List<FaceGroup> groups)
		{
			Results = groups;

			CollectionView.ReloadData ();
		}


		public override nint NumberOfSections (UICollectionView collectionView) => Results?.Count ?? 0;


		public override UICollectionReusableView GetViewForSupplementaryElement (UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
		{
			var groupResult = Results [indexPath.Section];
			var header = collectionView.Dequeue<SimpleCVHeader> (UICollectionElementKindSection.Header, indexPath);

			header.SetTitle (groupResult.Title);

			return header;
		}


		public override nint GetItemsCount (UICollectionView collectionView, nint section) => Results? [(int) section]?.Faces?.Count ?? 0;


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.Dequeue<FaceCVC> (indexPath);
			var face = Results [indexPath.Section].Faces [indexPath.Row];

			cell.SetFaceImage (face, face.GetImage ());

			return cell;
		}
	}
}