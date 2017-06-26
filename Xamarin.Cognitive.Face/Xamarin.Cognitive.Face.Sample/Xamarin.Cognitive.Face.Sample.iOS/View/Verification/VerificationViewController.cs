using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Extensions;
using Foundation;
using UIKit;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Model;
using NomadCode.UIExtensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class VerificationViewController : BaseViewController
	{
		class Segues
		{
			public const string SelectPerson = "SelectPerson";
		}

		FaceSelectionCollectionViewController Face1SelectionController => ChildViewControllers [0] as FaceSelectionCollectionViewController;
		FaceSelectionCollectionViewController Face2SelectionController => ChildViewControllers [1] as FaceSelectionCollectionViewController;

		public VerificationViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (FaceState.Current.Verification.Type == VerificationType.FaceAndPerson)
			{
				ChooseButton2.SetTitle ("Choose Person", UIControlState.Normal);
				PersonView.Hidden = false;
				PersonNameLabel.Text = string.Empty;

				//kill the 2nd CVC too
				Face2SelectionController.RemoveFromParentViewController ();
			}
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			VerifyButton.TouchUpInside += Verify;
			ChooseButton2.TouchUpInside += ChooseButton2Tapped;
			Face1SelectionController.FaceSelectionChanged += FaceSelectionChanged;
			if (ChildViewControllers.Length > 1) Face2SelectionController.FaceSelectionChanged += FaceSelectionChanged;

			if (FaceState.Current.Verification.Type == VerificationType.FaceAndPerson &&
				FaceState.Current.Verification.Person != null)
			{
				FaceState.Current.Verification.NeedsPerson = false;

				PersonNameLabel.Text = FaceState.Current.Verification.Person.Name;
				PersonImageView.Image = (FaceState.Current.Verification.Face ??
										 FaceState.Current.Verification.Person.Faces.FirstOrDefault ())?.GetThumbnailImage ();
			}
		}


		public override void ViewWillDisappear (bool animated)
		{
			VerifyButton.TouchUpInside -= Verify;
			ChooseButton2.TouchUpInside -= ChooseButton2Tapped;
			Face1SelectionController.FaceSelectionChanged -= FaceSelectionChanged;
			if (ChildViewControllers.Length > 1) Face2SelectionController.FaceSelectionChanged -= FaceSelectionChanged;

			base.ViewWillDisappear (animated);
		}


		partial void ChooseImage1Action (NSObject sender)
		{
			ChooseImage (Face1SelectionController).Forget ();
		}


		void ChooseButton2Tapped (object sender, EventArgs e)
		{
			switch (FaceState.Current.Verification.Type)
			{
				case VerificationType.FaceAndFace:

					ChooseImage (Face2SelectionController).Forget ();
					break;
				case VerificationType.FaceAndPerson:

					FaceState.Current.Verification.NeedsPerson = true;

					PerformSegue (Segues.SelectPerson, this);
					break;
			}
		}


		void FaceSelectionChanged (object sender, EventArgs e)
		{
			checkInputs ();
		}


		void checkInputs ()
		{
			switch (FaceState.Current.Verification.Type)
			{
				case VerificationType.FaceAndFace:
					VerifyButton.Enabled = Face1SelectionController.HasSelection && Face2SelectionController.HasSelection;
					break;
				case VerificationType.FaceAndPerson:
					VerifyButton.Enabled = Face1SelectionController.HasSelection &&
						FaceState.Current.Verification.Person != null &&
						FaceState.Current.Verification.Group != null;
					break;
			}
		}


		async Task ChooseImage (FaceSelectionCollectionViewController selectionController)
		{
			try
			{
				var image = await this.ShowImageSelectionDialog ();

				if (image != null)
				{
					//make sure the image is in the .Up position
					image.FixOrientation ();

					this.ShowHUD ("Detecting faces");

					var detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (image.AsJpegStream);

					if (detectedFaces.Count == 0)
					{
						this.ShowSimpleHUD ("No faces detected");
					}
					else
					{
						selectionController.SetDetectedFaces (image, detectedFaces);

						this.HideHUD ();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error using image selected");
			}
		}


		async void Verify (object sender, EventArgs e)
		{
			try
			{
				this.ShowHUD ("Verifying faces");

				VerifyResult result = null;
				string successMsg = "These two faces are from the same person.  The confidence is {0}";
				string failMsg = "These two faces are not from the same person.";

				switch (FaceState.Current.Verification.Type)
				{
					case VerificationType.FaceAndFace:
						result = await FaceClient.Shared.Verify (Face1SelectionController.SelectedFace, Face2SelectionController.SelectedFace);
						break;
					case VerificationType.FaceAndPerson:
						successMsg = $"This face looks like {FaceState.Current.Verification.Person.Name}.  ";
						successMsg += "The confidence is {0}";
						failMsg = $"This face does not look like {FaceState.Current.Verification.Person.Name}.";
						result = await FaceClient.Shared.Verify (Face1SelectionController.SelectedFace, FaceState.Current.Verification.Person, FaceState.Current.Verification.Group);
						break;
				}

				this.HideHUD ();

				if (result.IsIdentical)
				{
					this.ShowSimpleAlert (string.Format (successMsg, result.Confidence));
				}
				else
				{
					this.ShowSimpleAlert (failMsg);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error verifying the selected faces");
			}
		}
	}
}