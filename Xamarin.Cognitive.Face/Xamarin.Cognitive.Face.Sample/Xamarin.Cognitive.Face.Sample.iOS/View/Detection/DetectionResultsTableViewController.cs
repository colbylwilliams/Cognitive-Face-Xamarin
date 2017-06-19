using System;
using System.Collections.Generic;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class DetectionResultsTableViewController : BaseTableViewController
	{
		List<Model.Face> DetectedFaces;
		UIImage SourceImage;
		List<UIImage> thumbnails;

		public DetectionResultsTableViewController (IntPtr handle) : base (handle)
		{
		}


		protected override void Dispose (bool disposing)
		{
			cleanup ();

			base.Dispose (disposing);
		}


		void cleanup (bool disposeCurrentImage = false, bool clearImages = true)
		{
			if (clearImages && thumbnails != null)
			{
				thumbnails.ForEach (i => i.Dispose ());
				thumbnails.Clear ();
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


		public void SetResults (List<Model.Face> detectedFaces, UIImage sourceImage)
		{
			cleanup ();

			DetectedFaces = detectedFaces;
			SourceImage = sourceImage;

			thumbnails = detectedFaces.GenerateThumbnails (sourceImage);

			TableView.ReloadData ();
		}


		public override nint NumberOfSections (UITableView tableView) => 1;


		public override nint RowsInSection (UITableView tableView, nint section) => DetectedFaces?.Count ?? 0;


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.Dequeue<DetectionResultsTableViewCell> (indexPath);

			var face = DetectedFaces [indexPath.Row];

			cell.SetFace (face, thumbnails [indexPath.Row]);

			return cell;
		}
	}
}