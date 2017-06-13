using System;
using UIKit;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class IdentifyResultTableViewCell : UITableViewCell, IHandleResults<IdentificationResult>
	{
		public IdentifyResultTableViewCell (IntPtr handle) : base (handle)
		{
		}


		public void SetResult (IdentificationResult identifyResult)
		{
			FaceImageView.Image = identifyResult.Face?.GetImage ();
			PersonNameLabel.Text = identifyResult.CandidateResult?.Person?.Name;
			ConfidenceLabel.Text = $"Confidence: {identifyResult.CandidateResult?.Confidence.ToString ()}";
		}
	}
}