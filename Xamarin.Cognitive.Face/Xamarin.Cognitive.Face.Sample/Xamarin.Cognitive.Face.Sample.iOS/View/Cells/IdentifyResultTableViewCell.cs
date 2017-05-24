using System;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using UIKit;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class IdentifyResultTableViewCell : UITableViewCell, IHandleResults<IdentificationResult>
	{
		public IdentifyResultTableViewCell (IntPtr handle) : base (handle)
		{
		}


		public void SetResult (IdentificationResult result)
		{
			FaceImageView.Image = result.Face?.GetImage ();
			PersonNameLabel.Text = result.Person?.Name;
			ConfidenceLabel.Text = $"Confidence: {result.Confidence.ToString ()}";
		}
	}
}