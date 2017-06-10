using System;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FaceGroupingViewController : UIViewController
	{
		FaceSelectionCollectionViewController FaceSelectionController => ChildViewControllers [0] as FaceSelectionCollectionViewController;
		GroupingResultCollectionViewController GroupResultCVC => ChildViewControllers [1] as GroupingResultCollectionViewController;

		public FaceGroupingViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			FaceSelectionController.AllowSelection = false;
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			GroupButton.TouchUpInside += Group;
			FaceSelectionController.FaceSelectionChanged += FaceSelectionChanged;
		}


		public override void ViewWillDisappear (bool animated)
		{
			GroupButton.TouchUpInside -= Group;
			FaceSelectionController.FaceSelectionChanged -= FaceSelectionChanged;

			base.ViewWillDisappear (animated);
		}


		partial void AddFaceAction (NSObject sender)
		{
			ChooseImage ().Forget ();
		}


		void FaceSelectionChanged (object sender, EventArgs e)
		{
			checkInputs ();
		}


		void checkInputs ()
		{
			GroupButton.Enabled = FaceSelectionController.Faces?.Count > 0;
		}


		async Task ChooseImage ()
		{
			try
			{
				var image = await this.ShowImageSelectionDialog ();

				if (image != null)
				{
					//make sure the image is in the .Up position
					image.FixOrientation ();

					this.ShowHUD ("Detecting faces");

					var detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (image.AsStream);

					if (detectedFaces.Count == 0)
					{
						this.ShowSimpleHUD ("No faces detected");
					}
					else
					{
						FaceSelectionController.AppendDetectedFaces (image, detectedFaces);
						TotalFacesLabel.Text = $"Total faces: {FaceSelectionController.Faces.Count}";

						this.HideHUD ();
					}

					checkInputs ();
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error using image selected");
			}
		}


		async void Group (object sender, EventArgs e)
		{
			try
			{
				this.ShowHUD ("Grouping similar faces");

				var groupResult = await FaceClient.Shared.GroupFaces (FaceSelectionController.Faces);

				this.HideHUD ();

				if (groupResult.HasGroups)
				{
					GroupResultCVC.SetFaceGroupResults (groupResult);
				}
				else
				{
					this.ShowSimpleAlert ("No face groups found");
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error finding face groups");
			}
		}
	}
}