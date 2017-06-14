using System;
using UIKit;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class SimilarFaceResultsTableViewController : FaceResultsTableViewController<SimilarFaceResultTableViewCell, (SimilarFaceResult, UIImage)>
	{
		public SimilarFaceResultsTableViewController (IntPtr handle) : base (handle)
		{
		}
	}
}