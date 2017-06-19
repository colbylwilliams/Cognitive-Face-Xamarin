using System;
using System.Collections.Generic;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FaceSelectionCollectionViewController : ItemsPerRowCollectionViewController
	{
		public List<Model.Face> Faces { get; private set; } = new List<Model.Face> ();

		public Model.Face SelectedFace { get; private set; }

		public string ReturnSegue { get; set; }

		public bool AllowSelection { get; set; } = true;

		public event EventHandler FaceSelectionChanged;

		List<UIImage> thumbnailImages;

		public bool HasSelection => SelectedFace != null;


		public FaceSelectionCollectionViewController (IntPtr handle) : base (handle)
		{
		}


		protected override void Dispose (bool disposing)
		{
			cleanup ();

			base.Dispose (disposing);
		}


		void cleanup (bool final = true)
		{
			if (thumbnailImages != null)
			{
				thumbnailImages.ForEach (i => i.Dispose ());
				thumbnailImages.Clear ();
			}

			if (final)
			{
				FaceSelectionChanged = null;
				thumbnailImages = null;
			}
		}


		public void SetDetectedFaces (UIImage sourceImage, List<Model.Face> detectedFaces)
		{
			cleanup (false);

			Faces = detectedFaces;
			SelectedFace = null;

			thumbnailImages = detectedFaces.GenerateThumbnails (sourceImage);

			CollectionView.ReloadData ();
		}


		public void AppendDetectedFaces (UIImage sourceImage, List<Model.Face> detectedFaces)
		{
			Faces.AddRange (detectedFaces);

			thumbnailImages = detectedFaces.GenerateThumbnails (sourceImage, thumbnailImages);

			CollectionView.ReloadData ();
		}


		public UIImage GetThumbnailForFace (Model.Face face)
		{
			var index = Faces.IndexOf (face);

			if (index > -1 && thumbnailImages.Count > index)
			{
				return thumbnailImages [index];
			}

			return null;
		}


		public override nint NumberOfSections (UICollectionView collectionView) => 1;


		public override nint GetItemsCount (UICollectionView collectionView, nint section) => Faces?.Count ?? 0;


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.Dequeue<FaceCVC> (indexPath);

			var detectedFace = Faces [indexPath.Row];
			var image = thumbnailImages [indexPath.Row];

			cell.SetFaceImage (detectedFace, image);

			return cell;
		}


		public async override void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (AllowSelection)
			{
				SelectedFace = Faces [indexPath.Row];

				var cell = collectionView.CellForItem (indexPath);
				cell.Highlighted = true;

				if (ReturnSegue != null)
				{
					var result = await this.ShowTwoOptionAlert ("Please choose", "Do you want to use this face?");

					if (result)
					{
						PerformSegue (ReturnSegue, this);
						//clean up, AFTER we've returned and any caller could get the thumbnail
						cleanup ();
					}
				}
				else
				{
					FaceSelectionChanged?.Invoke (this, EventArgs.Empty);
				}
			}
		}


		public override void ItemDeselected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (AllowSelection)
			{
				SelectedFace = null;

				var cell = collectionView.CellForItem (indexPath);
				cell.Highlighted = false;

				FaceSelectionChanged?.Invoke (this, EventArgs.Empty);
			}
		}
	}
}