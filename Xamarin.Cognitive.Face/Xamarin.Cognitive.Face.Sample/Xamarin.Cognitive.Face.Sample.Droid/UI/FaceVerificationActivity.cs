using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Text;
using Java.Util;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Droid.Contract;
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/face_verification",
			  ParentActivity = typeof (VerificationMenuActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FaceVerificationActivity : AppCompatActivity
	{
		private static int REQUEST_SELECT_IMAGE_0 = 0;
		private static int REQUEST_SELECT_IMAGE_1 = 1;
		private UUID mFaceId0 = null;
		private UUID mFaceId1 = null;
		private Bitmap mBitmap0 = null;
		private Bitmap mBitmap1 = null;
		private FaceListAdapter mFaceListAdapter0 = null;
		private FaceListAdapter mFaceListAdapter1 = null;
		private ProgressDialog mProgressDialog = null;
		private Button select_image_0, select_image_1, view_log, verify = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_verification);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			select_image_0 = FindViewById<Button> (Resource.Id.select_image_0);
			select_image_1 = FindViewById<Button> (Resource.Id.select_image_1);
			view_log = FindViewById<Button> (Resource.Id.view_log);
			verify = FindViewById<Button> (Resource.Id.verify);

			ClearDetectedFaces (0);
			ClearDetectedFaces (1);
			SetVerifyButtonEnabledStatus (false);
			LogHelper.ClearVerificationLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			select_image_0.Click += Select_Image_0_Click;
			select_image_1.Click += Select_Image_1_Click;
			view_log.Click += View_Log_Click;
			verify.Click += Verify_Click;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			select_image_0.Click -= Select_Image_0_Click;
			select_image_1.Click -= Select_Image_1_Click;
			view_log.Click -= View_Log_Click;
			verify.Click -= Verify_Click;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			int index;

			if (requestCode == REQUEST_SELECT_IMAGE_0)
			{
				index = 0;
			}
			else if (requestCode == REQUEST_SELECT_IMAGE_1)
			{
				index = 1;
			}
			else
			{
				return;
			}

			if (resultCode == Result.Ok)
			{
				// If image is selected successfully, set the image URI and bitmap.
				Bitmap bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

				if (bitmap != null)
				{
					// Image is select but not detected, disable verification button.
					SetVerifyButtonEnabledStatus (false);

					ClearDetectedFaces (index);

					// Set the image to detect.
					if (index == 0)
					{
						mBitmap0 = bitmap;
						mFaceId0 = null;
					}
					else
					{
						mBitmap1 = bitmap;
						mFaceId1 = null;
					}

					// Add verification log.
					AddLog ("Image" + index + ": " + data.Data + " resized to " + bitmap.Width + "x" + bitmap.Height);

					// Start detecting in image.
					Detect (bitmap, index);
				}
			}
		}

		private void ClearDetectedFaces (int index)
		{
			ListView faceList = (ListView) FindViewById (index == 0 ? Resource.Id.list_faces_0 : Resource.Id.list_faces_1);
			faceList.Visibility = ViewStates.Gone;

			ImageView imageView = (ImageView) FindViewById (index == 0 ? Resource.Id.image_0 : Resource.Id.image_1);
			imageView.SetImageResource (Color.Transparent);
		}

		private void Select_Image_0_Click (object sender, EventArgs e)
		{
			SelectImage (0);
		}

		private void Select_Image_1_Click (object sender, EventArgs e)
		{
			SelectImage (1);
		}

		private void Verify_Click (object sender, EventArgs e)
		{
			SetAllButtonEnabledStatus (false);
			ExecuteVerification ();
		}

		private async void ExecuteVerification ()
		{
			VerifyResult verify_result = null;

			mProgressDialog.Show ();
			AddLog ("Request: Verifying face " + mFaceId0 + " and face " + mFaceId1);

			try
			{
				mProgressDialog.SetMessage ("Verifying...");
				SetInfo ("Verifying...");
				verify_result = await FaceClient.Shared.Verify (mFaceId0, mFaceId1);
			}
			catch (Java.Lang.Exception e)
			{
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (verify_result != null)
				 {
					 AddLog ("Response: Success. Face " + mFaceId0 + " and face "
							 + mFaceId1 + (verify_result.IsIdentical ? " " : " don't ")
							 + "belong to the same person");
				 }

				 // Show the result on screen when verification is done.
				 SetUiAfterVerification (verify_result);
			 });
		}

		private void View_Log_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (VerificationLogActivity));
			StartActivity (intent);
		}

		private void SelectImage (int index)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, index == 0 ? REQUEST_SELECT_IMAGE_0 : REQUEST_SELECT_IMAGE_1);
		}

		private void SetSelectImageButtonEnabledStatus (bool isEnabled, int index)
		{
			Button button;

			if (index == 0)
			{
				button = (Button) FindViewById (Resource.Id.select_image_0);
			}
			else
			{
				button = (Button) FindViewById (Resource.Id.select_image_1);
			}

			button.Enabled = isEnabled;

			Button viewLog = (Button) FindViewById (Resource.Id.view_log);
			viewLog.Enabled = isEnabled;
		}

		private void SetVerifyButtonEnabledStatus (bool isEnabled)
		{
			Button button = (Button) FindViewById (Resource.Id.verify);
			button.Enabled = isEnabled;
		}

		private void SetAllButtonEnabledStatus (bool isEnabled)
		{
			Button selectImage0 = (Button) FindViewById (Resource.Id.select_image_0);
			selectImage0.Enabled = isEnabled;

			Button selectImage1 = (Button) FindViewById (Resource.Id.select_image_1);
			selectImage1.Enabled = isEnabled;

			Button verif = (Button) FindViewById (Resource.Id.verify);
			verif.Enabled = isEnabled;

			Button viewLog = (Button) FindViewById (Resource.Id.view_log);
			viewLog.Enabled = isEnabled;
		}

		private void InitializeFaceList (int index)
		{
			ListView listView = (ListView) FindViewById (index == 0 ? Resource.Id.list_faces_0 : Resource.Id.list_faces_1);

			listView.ItemClick += (sender, e) =>
			{

				FaceListAdapter faceListAdapter = index == 0 ? mFaceListAdapter0 : mFaceListAdapter1;

				if (!faceListAdapter.faces [e.Position].FaceId.Equals (
						index == 0 ? mFaceId0 : mFaceId1))
				{
					if (index == 0)
					{
						mFaceId0 = faceListAdapter.faces [e.Position].FaceId;
					}
					else
					{
						mFaceId1 = faceListAdapter.faces [e.Position].FaceId;
					}

					ImageView imageView = (ImageView) FindViewById (index == 0 ? Resource.Id.image_0 : Resource.Id.image_1);
					imageView.SetImageBitmap (faceListAdapter.faceThumbnails [e.Position]);

					SetInfo ("");
				}

				// Show the list of detected face thumbnails.
				ListView lv = (ListView) FindViewById (index == 0 ? Resource.Id.list_faces_0 : Resource.Id.list_faces_1);
				lv.Adapter = faceListAdapter;
			};
		}

		private void SetUiAfterVerification (VerifyResult result)
		{
			mProgressDialog.Dismiss ();

			SetAllButtonEnabledStatus (true);

			// Show verification result.
			if (result != null)
			{
				DecimalFormat formatter = new DecimalFormat ("#0.00");
				string verificationResult = ((result.IsIdentical ? "The same person" : "Different persons") + ". The confidence is " + result.Confidence.ToString ());

				SetInfo ((string) verificationResult);
			}
		}

		private void SetUiAfterDetection (Face.Droid.Contract.Face [] result, int index, bool succeed)
		{
			SetSelectImageButtonEnabledStatus (true, index);

			if (succeed)
			{
				AddLog ("Response: Success. Detected " + result.Length + " face(s) in image" + index);
				SetInfo (result.Length + " face" + (result.Length != 1 ? "s" : "") + " detected");

				FaceListAdapter faceListAdapter = new FaceListAdapter (result, index, this);

				if (faceListAdapter.faces.Count != 0)
				{
					if (index == 0)
					{
						mFaceId0 = faceListAdapter.faces [0].FaceId;
					}
					else
					{
						mFaceId1 = faceListAdapter.faces [0].FaceId;
					}
					ImageView imageView = (ImageView) FindViewById (index == 0 ? Resource.Id.image_0 : Resource.Id.image_1);
					imageView.SetImageBitmap (faceListAdapter.faceThumbnails [0]);
				}

				ListView listView = (ListView) FindViewById (index == 0 ? Resource.Id.list_faces_0 : Resource.Id.list_faces_1);
				listView.Adapter = faceListAdapter;
				listView.Visibility = ViewStates.Visible;

				if (index == 0)
				{
					mFaceListAdapter0 = faceListAdapter;
					mBitmap0 = null;
				}
				else
				{
					mFaceListAdapter1 = faceListAdapter;
					mBitmap1 = null;
				}
			}

			if (result != null && result.Length == 0)
			{
				SetInfo ("No face detected!");
			}

			if ((index == 0 && mBitmap1 == null) || (index == 1 && mBitmap0 == null) || index == 2)
			{
				mProgressDialog.Dismiss ();
			}

			if (mFaceId0 != null && mFaceId1 != null)
			{

				SetVerifyButtonEnabledStatus (true);
			}
		}

		private void Detect (Bitmap bitmap, int index)
		{
			ExecuteDetection (index);
			SetSelectImageButtonEnabledStatus (false, index);
		}

		private async void ExecuteDetection (int mIndex)
		{
			Face.Droid.Contract.Face [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Detecting in image " + mIndex);

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					if (mIndex == 0)
					{
						mBitmap0.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);
					}
					else
					{
						mBitmap1.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);
					}

					using (ByteArrayInputStream inputStream = new ByteArrayInputStream (pre_output.ToArray ()))
					{
						byte [] arr = new byte [inputStream.Available ()];
						inputStream.Read (arr);
						var output = new MemoryStream (arr);

						mProgressDialog.SetMessage ("Detecting...");
						SetInfo ("Detecting...");
						faces = await FaceClient.Shared.Detect (output, true, true, new [] {
								  FaceServiceClientFaceAttributeType.Age,
								  FaceServiceClientFaceAttributeType.Gender,
								  FaceServiceClientFaceAttributeType.Smile,
								  FaceServiceClientFaceAttributeType.Glasses,
								  FaceServiceClientFaceAttributeType.FacialHair,
								  FaceServiceClientFaceAttributeType.Emotion,
								  FaceServiceClientFaceAttributeType.HeadPose
								});
					}
				}
			}
			catch (Java.Lang.Exception e)
			{
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 SetUiAfterDetection (faces, mIndex, mSucceed);
			 });
		}

		private void SetInfo (string _info)
		{
			TextView info = FindViewById<TextView> (Resource.Id.info);
			info.Text = _info;
		}

		private void AddLog (string _log)
		{
			LogHelper.AddVerificationLog (_log);
		}

		private class FaceListAdapter : BaseAdapter
		{
			public List<Face.Droid.Contract.Face> faces;
			public List<Bitmap> faceThumbnails;
			private int mIndex;
			private FaceVerificationActivity activity;

			public FaceListAdapter (Face.Droid.Contract.Face [] detectionResult, int index, FaceVerificationActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				mIndex = index;
				activity = act;

				if (detectionResult != null)
				{
					faces = detectionResult.ToList ();

					foreach (Face.Droid.Contract.Face face in faces)
					{
						try
						{
							faceThumbnails.Add (ImageHelper.GenerateFaceThumbnail (index == 0 ? activity.mBitmap0 : activity.mBitmap1, face.FaceRectangle));
						}
						catch (Java.IO.IOException ex)
						{
							activity.SetInfo (ex.Message);
						}
					}
				}
			}

			public override bool IsEnabled (int position)
			{
				return false;
			}

			public override int Count
			{
				get
				{
					return faces.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return faces [position];
			}

			public override long GetItemId (int position)
			{
				return position;
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					LayoutInflater layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face, parent, false);
				}
				convertView.Id = position;

				Bitmap thumbnailToShow = faceThumbnails [position];
				if (mIndex == 0 && faces [position].FaceId.Equals (activity.mFaceId0))
				{
					thumbnailToShow = ImageHelper.HighlightSelectedFaceThumbnail (thumbnailToShow);
				}
				else if (mIndex == 1 && faces [position].FaceId.Equals (activity.mFaceId1))
				{
					thumbnailToShow = ImageHelper.HighlightSelectedFaceThumbnail (thumbnailToShow);
				}

				// Show the face thumbnail.
				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageBitmap (thumbnailToShow);

				return convertView;
			}
		}
	}
}