using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class IdentifyFaceViewController : PopoverPresentationViewController
	{
		class Segues
		{
			public const string Embed = "Embed";
			public const string ShowResults = "ShowResults";
		}


		List<IdentificationResult> Results;

		FaceSelectionCollectionViewController FaceSelectionController => ChildViewControllers [0] as FaceSelectionCollectionViewController;
		GroupsTableViewController GroupsTableController => ChildViewControllers [1] as GroupsTableViewController;

		public IdentifyFaceViewController (IntPtr handle) : base (handle)
		{
		}


		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier == Segues.Embed && segue.DestinationViewController is GroupsTableViewController groupsTVC)
			{
				groupsTVC.AutoSelect = true;
			}
			else if (segue.Identifier == Segues.ShowResults && segue.DestinationViewController is IdentifyResultsTableViewController resultsTVC)
			{
				resultsTVC.PopoverPresentationController.WeakDelegate = this;
				resultsTVC.Results = Results;

				//segue.Dispose ();
			}
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			FaceSelectionController.FaceSelectionChanged += OnFaceSelectionChanged;
			GroupsTableController.GroupSelectionChanged += OnGroupSelectionChanged;
			GoButton.TouchUpInside += Identify;
		}


		public override void ViewWillDisappear (bool animated)
		{
			FaceSelectionController.FaceSelectionChanged -= OnFaceSelectionChanged;
			GroupsTableController.GroupSelectionChanged -= OnGroupSelectionChanged;
			GoButton.TouchUpInside -= Identify;

			base.ViewWillDisappear (animated);
		}


		void OnFaceSelectionChanged (object sender, EventArgs e)
		{
			checkInputs ();
		}


		void OnGroupSelectionChanged (object sender, PersonGroup selection)
		{
			checkInputs ();
		}


		partial void ChooseImageAction (NSObject sender)
		{
			ChooseImage ().Forget ();
		}


		async Task ChooseImage ()
		{
			try
			{
				var sourceImage = await this.ShowImageSelectionDialog ();

				if (sourceImage != null)
				{
					//make sure the image is in the .Up position
					sourceImage.FixOrientation ();

					this.ShowHUD ("Detecting faces");

					var detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (sourceImage.AsStream);

					if (detectedFaces.Count == 0)
					{
						this.ShowSimpleHUD ("No faces detected");
					}
					else
					{
						this.HideHUD ();

						FaceSelectionController.SetDetectedFaces (sourceImage, detectedFaces);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error detecting faces in the provided image");
			}
		}


		void checkInputs ()
		{
			GoButton.Enabled = FaceSelectionController.SelectedFace != null &&
				GroupsTableController.SelectedPersonGroup != null;
		}


		async void Identify (object sender, EventArgs e)
		{
			try
			{
				var group = GroupsTableController.SelectedPersonGroup;

				this.ShowHUD ("Identifying faces");

				Results = await FaceClient.Shared.Identify (group, FaceSelectionController.SelectedFace);

				if (Results.Any (r => r.HasCandidates))
				{
					this.HideHUD ();

					PerformSegue (Segues.ShowResults, this);
				}
				else
				{
					this.ShowSimpleHUD ("Not able to identify this face");
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				this.HideHUD ().ShowSimpleAlert ("Error identifying face");
			}
		}


		protected override string GetPopoverCloseText (UIViewController presentedViewController)
		{
			return "Done";
		}
	}
}