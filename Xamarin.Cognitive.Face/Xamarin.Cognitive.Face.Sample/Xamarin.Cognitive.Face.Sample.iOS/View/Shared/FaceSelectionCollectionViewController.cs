using System;
using System.Collections.Generic;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FaceSelectionCollectionViewController : ItemsPerRowCollectionViewController
	{
		public List<Shared.Face> Faces { get; set; } = new List<Shared.Face> ();
		public UIImage SourceImage { get; set; }
		public string ReturnSegue { get; set; }
		public Shared.Face SelectedFace { get; private set; }
		public bool AllowSelection { get; set; } = true;

		public event EventHandler FaceSelectionChanged;

		List<UIImage> croppedImages;

		public bool HasSelection => SelectedFace != null;


		public FaceSelectionCollectionViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (SourceImage != null)
			{
				addCroppedImages (Faces);
			}
		}


		protected override void Dispose (bool disposing)
		{
			cleanup ();

			base.Dispose (disposing);
		}


		void cleanup (bool disposeCurrentImage = false, bool clearImages = true)
		{
			if (clearImages && croppedImages != null)
			{
				croppedImages.ForEach (i => i.Dispose ());
				croppedImages.Clear ();
			}

			if (SourceImage != null)
			{
				if (disposeCurrentImage)
				{
					SourceImage.Dispose ();
				}

				SourceImage = null;
			}
		}


		public void SetDetectedFaces (UIImage sourceImage, List<Shared.Face> detectedFaces, bool append = false)
		{
			cleanup (true);

			SourceImage = sourceImage;
			Faces = detectedFaces;
			SelectedFace = null;

			addCroppedImages (detectedFaces);

			CollectionView.ReloadData ();
		}


		public void AppendDetectedFaces (UIImage sourceImage, List<Shared.Face> detectedFaces, bool append = false)
		{
			cleanup (true, false);

			SourceImage = sourceImage;
			Faces.AddRange (detectedFaces);

			addCroppedImages (detectedFaces, croppedImages);

			CollectionView.ReloadData ();
		}


		void addCroppedImages (List<Shared.Face> detectedFaces, List<UIImage> images = null)
		{
			croppedImages = images ?? new List<UIImage> ();

			foreach (var face in detectedFaces)
			{
				croppedImages.Add (SourceImage.Crop (face.FaceRectangle));
			}
		}


		public override nint NumberOfSections (UICollectionView collectionView) => 1;


		public override nint GetItemsCount (UICollectionView collectionView, nint section) => Faces?.Count ?? 0;


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.Dequeue<FaceCVC> (indexPath);

			var detectedFace = Faces [indexPath.Row];
			var image = croppedImages [indexPath.Row];

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
						//cleanup first
						cleanup ();

						PerformSegue (ReturnSegue, this);
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