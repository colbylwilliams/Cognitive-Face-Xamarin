using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FindSimilarFacesViewController : PopoverPresentationViewController
	{
		class Segues
		{
			public const string ShowResults = "ShowResults";
		}

		List<SimilarFaceResult> Results;

		FaceSelectionCollectionViewController Face1SelectionController => ChildViewControllers [0] as FaceSelectionCollectionViewController;
		FaceSelectionCollectionViewController Face2SelectionController => ChildViewControllers [1] as FaceSelectionCollectionViewController;

		public FindSimilarFacesViewController (IntPtr handle) : base (handle)
		{
		}


		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier == Segues.ShowResults && segue.DestinationViewController is SimilarFaceResultsTableViewController resultsTVC)
			{
				resultsTVC.PopoverPresentationController.Delegate = this;
				resultsTVC.Results = Results;
			}
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Face1SelectionController.AllowSelection = false;
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			FindButton.TouchUpInside += Find;
			Face1SelectionController.FaceSelectionChanged += FaceSelectionChanged;
			Face2SelectionController.FaceSelectionChanged += FaceSelectionChanged;
		}


		public override void ViewWillDisappear (bool animated)
		{
			FindButton.TouchUpInside -= Find;
			Face1SelectionController.FaceSelectionChanged -= FaceSelectionChanged;
			Face2SelectionController.FaceSelectionChanged -= FaceSelectionChanged;

			base.ViewWillDisappear (animated);
		}


		partial void AddFaceAction (NSObject sender)
		{
			ChooseImage (Face1SelectionController).Forget ();
		}


		partial void ChooseImageAction (NSObject sender)
		{
			ChooseImage (Face2SelectionController).Forget ();
		}


		void FaceSelectionChanged (object sender, EventArgs e)
		{
			checkInputs ();
		}


		void checkInputs ()
		{
			FindButton.Enabled = Face1SelectionController.Faces?.Count > 0 && Face2SelectionController.HasSelection;
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

					var detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (image);

					if (detectedFaces.Count == 0)
					{
						this.ShowSimpleHUD ("No faces detected");
					}
					else
					{
						if (selectionController == Face1SelectionController)
						{
							selectionController.AppendDetectedFaces (image, detectedFaces);
							TotalFacesLabel.Text = $"Total faces: {selectionController.Faces.Count}";
						}
						else //Face2SelectionController
						{
							selectionController.SetDetectedFaces (image, detectedFaces);
						}

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


		async void Find (object sender, EventArgs e)
		{
			try
			{
				this.ShowHUD ("Finding similar faces");

				Results = await FaceClient.Shared.FindSimilar (Face2SelectionController.SelectedFace, Face1SelectionController.Faces);

				this.HideHUD ();

				if (Results.Count > 0)
				{
					PerformSegue (Segues.ShowResults, this);
				}
				else
				{
					this.ShowSimpleAlert ("No similar faces found");
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error finding similar faces");
			}
		}


		protected override string GetPopoverCloseText (UIViewController presentedViewController)
		{
			return "Done";
		}
	}
}