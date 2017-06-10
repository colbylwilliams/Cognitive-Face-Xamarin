using System;
using Xamarin.Cognitive.Face.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class SimilarFaceResultsTableViewController : FaceResultsTableViewController<SimilarFaceResultTableViewCell, SimilarFaceResult>
	{
		public SimilarFaceResultsTableViewController (IntPtr handle) : base (handle)
		{
		}
	}
}