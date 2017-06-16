using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class PersonDetailViewController : PopoverPresentationViewController
	{
		class Segues
		{
			public const string Embed = "Embed";
			public const string SelectFaces = "SelectFaces";
			public const string FaceSelected = "PersonFaceSelected";
			public const string VerifyPerson = "VerifyPerson";
		}

		public PersonGroup Group => FaceState.Current.CurrentGroup;
		public Person Person => FaceState.Current.CurrentPerson;

		public List<Model.Face> DetectedFaces { get; set; }
		public UIImage SourceImage { get; set; }

		PersonFaceCollectionViewController PersonFaceCVC => ChildViewControllers [0] as PersonFaceCollectionViewController;

		public PersonDetailViewController (IntPtr handle) : base (handle)
		{
		}


		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier == Segues.SelectFaces && segue.DestinationViewController is FaceSelectionCollectionViewController faceSelectionController)
			{
				faceSelectionController.PopoverPresentationController.Delegate = this;
				faceSelectionController.ReturnSegue = Segues.FaceSelected;
				faceSelectionController.Faces = DetectedFaces;
				faceSelectionController.SourceImage = SourceImage;
			}
		}


		[Action ("UnwindToPersonDetail:")]
		public async void UnwindToPersonDetail (UIStoryboardSegue segue)
		{
			var faceSelection = segue.SourceViewController as FaceSelectionCollectionViewController;

			if (faceSelection.SelectedFace != null)
			{
				await AddFace (faceSelection.SelectedFace, SourceImage);
			}
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (Person != null)
			{
				PersonName.Text = Person.Name;
			}
		}


		partial void SaveAction (NSObject sender)
		{
			if (PersonName.Text.Length == 0)
			{
				this.ShowSimpleAlert ("Please input the person's name");
				return;
			}

			if (Person == null)
			{
				CreateNewPerson ().Forget ();
			}
			else
			{
				UpdatePerson ().Forget ();
			}
		}


		async Task CreateNewPerson ()
		{
			try
			{
				this.ShowHUD ("Creating person");

				FaceState.Current.CurrentPerson = await FaceClient.Shared.CreatePerson (PersonName.Text, Group);

				PersonFaceCVC.CollectionView.ReloadData ();

				this.ShowSimpleHUD ("Person created");
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to create person.");
			}
		}


		async Task UpdatePerson ()
		{
			try
			{
				this.ShowHUD ("Saving person");

				await FaceClient.Shared.UpdatePerson (Person, Group, PersonName.Text);

				PersonFaceCVC.CollectionView.ReloadData ();

				this.ShowSimpleHUD ("Person saved");
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to update person.");
			}
		}


		partial void AddFaceAction (NSObject sender)
		{
			if (PersonName.Text.Length == 0)
			{
				this.ShowSimpleAlert ("Please input the person's name");
				return;
			}

			AddFace ().Forget ();
		}


		async Task AddFace ()
		{
			if (Person == null)
			{
				var createPerson = await this.ShowTwoOptionAlert ("Create Person?", "Do you want to create this new person?");

				if (!createPerson)
				{
					return;
				}

				await CreateNewPerson ();
			}

			if (Person != null) //just to make sure we succeeded in the case we created a new person above
			{
				var image = await this.ShowImageSelectionDialog ();

				if (image != null)
				{
					SourceImage = image;
					await DetectFaces ();
				}
			}
		}


		async Task DetectFaces ()
		{
			try
			{
				this.ShowHUD ("Detecting faces");

				DetectedFaces = await FaceClient.Shared.DetectFacesInPhoto (SourceImage.AsStream);

				if (DetectedFaces.Count == 0)
				{
					this.ShowSimpleHUD ("No faces detected");
				}
				else if (DetectedFaces.Count == 1)
				{
					await AddFace (DetectedFaces [0], SourceImage);
				}
				else // > 1 face
				{
					this.HideHUD ();

					PerformSegue (Segues.SelectFaces, this);
				}
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Face detection failed");
			}
		}


		async Task AddFace (Model.Face face, UIImage sourceImage)
		{
			try
			{
				this.ShowHUD ("Adding face");

				await FaceClient.Shared.AddFaceForPerson (Person, Group, face, sourceImage.AsStream);

				face.SaveThumbnailFromSource (sourceImage);

				this.ShowSimpleHUD ("Face added for this person");

				PersonFaceCVC.CollectionView.ReloadData ();

				FaceState.Current.NeedsTraining = true;
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Failed to add face.");
			}
		}


		partial void VerifyAction (NSObject sender)
		{
			FaceState.Current.Verification.Person = Person;
			FaceState.Current.Verification.Group = Group;
			FaceState.Current.Verification.Type = VerificationType.FaceAndPerson;

			PerformSegue (Segues.VerifyPerson, this);
		}
	}
}