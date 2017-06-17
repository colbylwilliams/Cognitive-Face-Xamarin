using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/face_verification",
			  ParentActivity = typeof (VerificationMenuActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FaceVerificationActivity : AppCompatActivity, AdapterView.IOnItemClickListener
	{
		Model.Face [] Faces = new Model.Face [2];
		Bitmap [] Bitmaps = new Bitmap [2];
		ListView [] ListViews = new ListView [2];
		Button [] SelectImageButtons = new Button [2];
		Button view_log, verify;
		ProgressDialog progressDialog;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_verification);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			SelectImageButtons [0] = FindViewById<Button> (Resource.Id.select_image_0);
			SelectImageButtons [1] = FindViewById<Button> (Resource.Id.select_image_1);
			view_log = FindViewById<Button> (Resource.Id.view_log);
			verify = FindViewById<Button> (Resource.Id.verify);

			ListViews [0] = FindViewById<ListView> (Resource.Id.list_faces_0);
			ListViews [0].Tag = 0;
			ListViews [1] = FindViewById<ListView> (Resource.Id.list_faces_1);
			ListViews [1].Tag = 1;

			ClearDetectedFaces (0);
			ClearDetectedFaces (1);

			SetVerifyButtonEnabledStatus (false);
			LogHelper.ClearLog (LogType.Verification);
		}


		protected override void OnResume ()
		{
			base.OnResume ();

			SelectImageButtons [0].Click += Select_Image_Click;
			SelectImageButtons [1].Click += Select_Image_Click;
			view_log.Click += View_Log_Click;
			verify.Click += Verify_Click;
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			SelectImageButtons [0].Click -= Select_Image_Click;
			SelectImageButtons [1].Click -= Select_Image_Click;
			view_log.Click -= View_Log_Click;
			verify.Click -= Verify_Click;
		}


		protected async override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			int index = requestCode;

			if (resultCode == Result.Ok)
			{
				// If image is selected successfully, set the image bitmap.
				var bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

				if (bitmap != null)
				{
					// Image is select but not detected, disable verification button.
					SetVerifyButtonEnabledStatus (false);

					ClearDetectedFaces (index);

					Bitmaps [index] = bitmap;
					Faces [index] = null;

					AddLog ($"Image{index}: {data.Data} resized to {bitmap.Width} x {bitmap.Height}");

					// Start detecting in image.
					await Detect (bitmap, index);
				}
			}
		}


		ListView ListViewAtIndex (int index)
		{
			return ListViews [index];
		}


		ImageView ImageViewAtIndex (int index)
		{
			return FindViewById<ImageView> (index == 0 ? Resource.Id.image_0 : Resource.Id.image_1);
		}


		Button SelectButtonAtIndex (int index)
		{
			return SelectImageButtons [index];
		}


		void ClearDetectedFaces (int index)
		{
			ListViewAtIndex (index).Visibility = ViewStates.Gone;
			ImageViewAtIndex (index).SetImageResource (Color.Transparent);
		}


		void Select_Image_Click (object sender, EventArgs e)
		{
			var index = Array.IndexOf (SelectImageButtons, sender);
			SelectImage (index);
		}


		public void OnItemClick (AdapterView parent, View view, int position, long id)
		{
			var listView = (ListView) parent;
			var faceListAdapter = listView.Adapter as FaceImageListAdapter;
			var selectedFace = faceListAdapter [position];
			var index = (int) listView.Tag;

			if (selectedFace != Faces [0] && selectedFace != Faces [1])
			{
				var image = faceListAdapter.GetThumbnailForPosition (position);
				ImageViewAtIndex (index).SetImageBitmap (image);

				Faces [index] = selectedFace;
				faceListAdapter.SetSelectedIndex (position);

				SetInfo ("");
			}
		}


		async void Verify_Click (object sender, EventArgs e)
		{
			SetAllButtonEnabledStatus (false);

			await ExecuteVerification ();
		}


		async Task ExecuteVerification ()
		{
			try
			{
				AddLog ($"Request: Verifying face {Faces [0].Id} and face {Faces [1].Id}");
				progressDialog.Show ();
				progressDialog.SetMessage ("Verifying...");
				SetInfo ("Verifying...");

				var result = await FaceClient.Shared.Verify (Faces [0], Faces [1]);

				if (result != null)
				{
					AddLog ($"Response: Success. Face {Faces [0].Id} and face {Faces [1].Id} {(result.IsIdentical ? "" : " don't")} belong to the same person");

					SetAllButtonEnabledStatus (true);

					// Show verification result.
					string verificationResult = $"{(result.IsIdentical ? "The same person" : "Different persons")}. The confidence is {result.Confidence}";

					SetInfo (verificationResult);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (VerificationLogActivity));
			StartActivity (intent);
		}


		void SelectImage (int index)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, index);
		}


		void SetSelectImageButtonEnabledStatus (bool isEnabled, int index)
		{
			SelectButtonAtIndex (index).Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void SetVerifyButtonEnabledStatus (bool isEnabled)
		{
			verify.Enabled = isEnabled;
		}


		void SetAllButtonEnabledStatus (bool isEnabled)
		{
			SelectImageButtons [0].Enabled = isEnabled;
			SelectImageButtons [1].Enabled = isEnabled;
			verify.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		async Task Detect (Bitmap bitmap, int index)
		{
			await ExecuteDetection (bitmap, index);

			SetSelectImageButtonEnabledStatus (false, index);
		}


		async Task ExecuteDetection (Bitmap bitmap, int index)
		{
			try
			{
				AddLog ($"Request: Detecting in image {index}");
				progressDialog.Show ();
				progressDialog.SetMessage ("Detecting...");
				SetInfo ("Detecting...");

				var faces = await FaceClient.Shared.DetectFacesInPhoto (
					() => bitmap.AsJpeg (),
					true,
					FaceAttributeType.Age,
					FaceAttributeType.Gender,
					FaceAttributeType.Smile,
					FaceAttributeType.Glasses,
					FaceAttributeType.FacialHair,
					FaceAttributeType.Emotion,
					FaceAttributeType.HeadPose
				);

				AddLog ($"Response: Success. Detected {faces.Count} face(s) in image" + index);
				SetInfo ($"{faces.Count} face{(faces.Count != 1 ? "s" : "")} detected");

				var faceListAdapter = new FaceImageListAdapter (faces, bitmap);

				if (faces?.Count != 0)
				{
					Faces [index] = faces.First ();
					ImageViewAtIndex (index).SetImageBitmap (faceListAdapter.GetThumbnailForPosition (0));
					faceListAdapter.SetSelectedIndex (0);
				}
				else
				{
					SetInfo ("No face detected!");
				}

				var listView = ListViewAtIndex (index);
				listView.Adapter = faceListAdapter;
				listView.Visibility = ViewStates.Visible;
				listView.OnItemClickListener = this;

				Bitmaps [index] = null;

				if (Faces [0] != null && Faces [1] != null)
				{
					SetVerifyButtonEnabledStatus (true);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("No face detected!");
			}

			SetSelectImageButtonEnabledStatus (true, index);

			progressDialog.Dismiss ();
		}


		void SetInfo (string info)
		{
			var txtInfo = FindViewById<TextView> (Resource.Id.info);
			txtInfo.Text = info;
		}


		void AddLog (string log)
		{
			LogHelper.AddLog (LogType.Verification, log);
		}
	}
}