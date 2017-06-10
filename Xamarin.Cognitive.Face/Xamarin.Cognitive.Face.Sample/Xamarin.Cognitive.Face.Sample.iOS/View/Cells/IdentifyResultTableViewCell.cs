using System;
using Xamarin.Cognitive.Face.Extensions;
using UIKit;
using Xamarin.Cognitive.Face.Shared;

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