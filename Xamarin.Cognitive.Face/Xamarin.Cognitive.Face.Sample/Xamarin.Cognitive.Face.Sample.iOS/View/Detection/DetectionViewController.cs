using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class DetectionViewController : BaseViewController
	{
		class Segues
		{
			public const string Embed = "Embed";
		}

		public UIImage SourceImage { get; set; }
		public List<Shared.Face> DetectedFaces { get; set; }

		DetectionResultsTableViewController DetectionResultsController => ChildViewControllers [0] as DetectionResultsTableViewController;

		public DetectionViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			DetectButton.TouchUpInside += Detect;
		}


		public override void ViewWillDisappear (bool animated)
		{
			DetectButton.TouchUpInside -= Detect;

			base.ViewWillDisappear (animated);
		}


		partial void ChooseImageAction (NSObject sender)
		{
			ChooseImage ().Forget ();
		}


		void checkInputs ()
		{
			DetectButton.Enabled = SourceImage != null;
		}


		async Task ChooseImage ()
		{
			try
			{
				SourceImage = await this.ShowImageSelectionDialog ();

				if (SourceImage != null)
				{
					//make sure the image is in the .Up position
					SourceImage.FixOrientation ();

					DetectionImageView.Image = SourceImage;

					checkInputs ();
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error using image selected");
			}
		}


		async void Detect (object sender, EventArgs e)
		{
			try
			{
				this.ShowHUD ("Detecting faces");

				DetectedFaces = await FaceClient.Shared.DetectFacesInPhoto (SourceImage,
																			MPOFaceAttributeType.Age,
																			MPOFaceAttributeType.Gender,
																			MPOFaceAttributeType.Hair,
																			MPOFaceAttributeType.FacialHair,
																			MPOFaceAttributeType.Makeup,
																			MPOFaceAttributeType.Emotion,
																			MPOFaceAttributeType.Occlusion,
																			MPOFaceAttributeType.Smile,
																			MPOFaceAttributeType.Exposure,
																			MPOFaceAttributeType.Noise,
																			MPOFaceAttributeType.Blur,
																			MPOFaceAttributeType.Glasses,
																			MPOFaceAttributeType.HeadPose,
																			MPOFaceAttributeType.Accessories);

				if (DetectedFaces.Count == 0)
				{
					this.ShowSimpleHUD ("No faces detected");
				}
				else // > 1 face
				{
					this.HideHUD ();

					DetectionResultsController.SetResults (DetectedFaces);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error detecting faces in the selected image");
			}
		}
	}
}