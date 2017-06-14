using System;
using Xamarin.Cognitive.Face.Extensions;
using UIKit;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class SimilarFaceResultTableViewCell : UITableViewCell, IHandleResults<SimilarFaceResult>
	{
		public SimilarFaceResultTableViewCell (IntPtr handle) : base (handle)
		{
		}


		public void SetResult (SimilarFaceResult result)
		{
			FaceImageView.Image = result.Face?.GetImage ();
			ConfidenceLabel.Text = $"Confidence: {result.Confidence.ToString ()}";
		}
	}
}