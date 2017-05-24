using System;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class DetectionResultsTableViewCell : UITableViewCell
	{
		public DetectionResultsTableViewCell (IntPtr handle) : base (handle)
		{
		}


		public void SetFace (Shared.Face face)
		{
			ImageView.Image = face.GetImage ();
			TitleLabel.Text = face.Id;
			SizeLabel.Text = $"Position: {face.FaceRectangle.Left},{face.FaceRectangle.Top}; Size: {face.FaceRectangle.Width}x{face.FaceRectangle.Height}";

			var attrs = face.Attributes;

			if (attrs != null)
			{
				AgeLabel.Text = $"Age: {attrs.Age}";
				GenderLabel.Text = $"Gender: {attrs.Gender}";
				HairLabel.Text = attrs.Hair?.ToString ();
				SmileLabel.Text = $"Smile Intensity: {attrs.SmileIntensity}";
				FacialHairLabel.Text = attrs.FacialHair?.ToString ();
				GlassesLabel.Text = $"Glasses: {attrs.Glasses}";
				EmotionLabel.Text = attrs.Emotion?.ToString ();
				MakeupLabel.Text = attrs.Makeup?.ToString ();

				HeadPoseLabel.Text = attrs.HeadPose?.ToString ();
				AccessoriesLabel.Text = attrs.Accessories?.ToString ();
				OcclusionLabel.Text = attrs.Occlusion?.ToString ();
				BlurLabel.Text = attrs.Blur?.ToString ();
				NoiseLabel.Text = attrs.Noise?.ToString ();
				ExposureLabel.Text = attrs.Exposure?.ToString ();
			}
		}
	}
}