using System;
using System.Collections.Generic;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class DetectionResultsTableViewController : BaseTableViewController
	{
		public List<Shared.Face> DetectedFaces { get; set; }

		public DetectionResultsTableViewController (IntPtr handle) : base (handle)
		{
		}


		public void SetResults (List<Shared.Face> detectedFaces)
		{
			DetectedFaces = detectedFaces;
			TableView.ReloadData ();
		}


		public override nint NumberOfSections (UITableView tableView) => 1;


		public override nint RowsInSection (UITableView tableView, nint section) => DetectedFaces?.Count ?? 0;


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.Dequeue<DetectionResultsTableViewCell> (indexPath);

			var face = DetectedFaces [indexPath.Row];

			cell.SetFace (face);

			return cell;
		}
	}
}