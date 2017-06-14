using System;
using UIKit;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class SimilarFaceResultTableViewCell : UITableViewCell, IHandleResults<(SimilarFaceResult, UIImage)>
	{
		public SimilarFaceResultTableViewCell (IntPtr handle) : base (handle)
		{
		}


		public void SetResult ((SimilarFaceResult, UIImage) result)
		{
			FaceImageView.Image = result.Item2;
			ConfidenceLabel.Text = $"Confidence: {result.Item1.Confidence.ToString ()}";
		}
	}
}