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

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person_verification",
			  ParentActivity = typeof (VerificationMenuActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonVerificationActivity : AppCompatActivity
	{
		private static int REQUEST_SELECT_IMAGE = 0;
		private UUID mFaceId, mPersonId = null;
		private Bitmap mBitmap = null;
		private FaceListAdapter mFaceListAdapter = null;
		private ProgressDialog mProgressDialog = null;
		private String mPersonGroupId = null;
		private PersonListAdapter mPersonListAdapter = null;
		private ListView listView_persons, listView_faces_0 = null;
		private Button select_image_0, manage_persons, verify, view_log = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_verification_person);

			listView_persons = (ListView) FindViewById (Resource.Id.list_persons);
			listView_faces_0 = (ListView) FindViewById (Resource.Id.list_faces_0);
			select_image_0 = (Button) FindViewById (Resource.Id.select_image_0);
			manage_persons = (Button) FindViewById (Resource.Id.manage_persons);
			verify = (Button) FindViewById (Resource.Id.verify);
			view_log = (Button) FindViewById (Resource.Id.view_log);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			ClearDetectedFaces ();
			SetVerifyButtonEnabledStatus (false);
			LogHelper.ClearVerificationLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			listView_persons.ItemClick += ListView_Persons_ItemClick;
			listView_faces_0.ItemClick += ListView_Faces_0_ItemClick;
			select_image_0.Click += Select_Image_0_Click;
			manage_persons.Click += Manage_Persons_Click;
			verify.Click += Verify_Click;
			view_log.Click += View_Log_Click;

			mPersonListAdapter = new PersonListAdapter (this);
			listView_persons.Adapter = mPersonListAdapter;

			if (mPersonListAdapter.personIdList.Count != 0)
			{
				SetPersonSelected (0);
			}
			else
			{
				SetPersonSelected (-1);
			}
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			listView_persons.ItemClick -= ListView_Persons_ItemClick;
			listView_faces_0.ItemClick -= ListView_Faces_0_ItemClick;
			select_image_0.Click -= Select_Image_0_Click;
			manage_persons.Click -= Manage_Persons_Click;
			verify.Click -= Verify_Click;
			view_log.Click -= View_Log_Click;
		}

		private void SetPersonSelected (int position)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.text_person_selected);
			if (position > 0)
			{
				String personGroupIdSelected = mPersonListAdapter.personGroupIds [position];
				mPersonListAdapter.personGroupIds [position] = mPersonListAdapter.personGroupIds [0];
				mPersonListAdapter.personGroupIds [0] = personGroupIdSelected;

				String personIdSelected = mPersonListAdapter.personIdList [position];
				mPersonListAdapter.personIdList [position] = mPersonListAdapter.personIdList [0];
				mPersonListAdapter.personIdList [0] = personIdSelected;

				ListView listView = (ListView) FindViewById (Resource.Id.list_persons);
				listView.Adapter = mPersonListAdapter;
				SetPersonSelected (0);
			}
			else if (position < 0)
			{
				SetVerifyButtonEnabledStatus (false);
				textView.SetTextColor (Color.Red);
				textView.Text = "no person selected for verification warning";
			}
			else
			{
				mPersonGroupId = mPersonListAdapter.personGroupIds [0];
				mPersonId = UUID.FromString (mPersonListAdapter.personIdList [0]);
				String personName = StorageHelper.GetPersonName (mPersonId.ToString (), mPersonGroupId, this);

				RefreshVerifyButtonEnabledStatus ();
				textView.SetTextColor (Color.Black);
				textView.Text = String.Format ("Person to use: {0}", personName);
			}
		}

		private void RefreshVerifyButtonEnabledStatus ()
		{
			if (mFaceId != null && mPersonId != null)
			{
				SetVerifyButtonEnabledStatus (true);
			}
			else
			{
				SetVerifyButtonEnabledStatus (false);
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode != REQUEST_SELECT_IMAGE)
			{
				return;
			}

			if (resultCode == Result.Ok)
			{
				Bitmap bitmap = ImageHelper.LoadSizeLimitedBitmapFromUri (data.Data, this.ContentResolver);

				if (bitmap != null)
				{
					SetVerifyButtonEnabledStatus (false);
					ClearDetectedFaces ();

					mBitmap = bitmap;
					mFaceId = null;

					AddLog ("Image" + ": " + data.Data + " resized to " + bitmap.Width + "x" + bitmap.Height);

					Detect ();
				}
			}
		}

		private void ClearDetectedFaces ()
		{
			ListView faceList = (ListView) FindViewById (Resource.Id.list_faces_0);
			faceList.Visibility = ViewStates.Gone;

			ImageView imageView = (ImageView) FindViewById (Resource.Id.image_0);
			imageView.SetImageResource (Color.Transparent);
		}

		private void Select_Image_0_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
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
			AddLog ("Request: Verifying face " + mFaceId + " and person " + mPersonId);

			try
			{
				mProgressDialog.SetMessage ("Verifying...");
				SetInfo ("Verifying...");
				verify_result = await FaceClient.Shared.Verify (mFaceId, mPersonGroupId, mPersonId);
			}
			catch (Java.Lang.Exception e)
			{
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (verify_result != null)
				 {
					 AddLog ("Response: Success. Face " + mFaceId + " "
							 + mPersonId + (verify_result.IsIdentical ? " " : " don't ")
							 + "belong to person " + mPersonId);
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

		private void SetSelectImageButtonEnabledStatus (bool isEnabled)
		{
			Button button = (Button) FindViewById (Resource.Id.select_image_0);
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

			Button selectImage1 = (Button) FindViewById (Resource.Id.manage_persons);
			selectImage1.Enabled = isEnabled;

			Button verify = (Button) FindViewById (Resource.Id.verify);
			verify.Enabled = isEnabled;

			Button viewLog = (Button) FindViewById (Resource.Id.view_log);
			viewLog.Enabled = isEnabled;
		}

		private void ListView_Persons_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			SetPersonSelected (e.Position);
		}

		private void ListView_Faces_0_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			FaceListAdapter faceListAdapter = mFaceListAdapter;

			if (!faceListAdapter.faces [e.Position].FaceId.Equals (mFaceId))
			{
				mFaceId = faceListAdapter.faces [e.Position].FaceId;

				ImageView imageView = (ImageView) FindViewById (Resource.Id.image_0);
				imageView.SetImageBitmap (faceListAdapter.faceThumbnails [e.Position]);

				SetInfo ("");
			}

			ListView listView = (ListView) FindViewById (Resource.Id.list_faces_0);
			listView.Adapter = faceListAdapter;
		}

		private void SetUiAfterVerification (VerifyResult result)
		{
			mProgressDialog.Dismiss ();
			SetAllButtonEnabledStatus (true);

			if (result != null)
			{
				DecimalFormat formatter = new DecimalFormat ("#0.00");
				String verificationResult = (result.IsIdentical ? "The same person" : "Different persons")
						+ ". The confidence is " + formatter.Format (result.Confidence);
				SetInfo (verificationResult);
			}
		}

		private void SetUiAfterDetection (Face.Droid.Contract.Face [] result, bool succeed)
		{
			SetSelectImageButtonEnabledStatus (true);

			if (succeed)
			{
				AddLog ("Response: Success. Detected " + result.Length + " face(s) in image");

				SetInfo (result.Length + " face" + (result.Length != 1 ? "s" : "") + " detected");

				// Show the detailed list of detected faces.
				FaceListAdapter faceListAdapter = new FaceListAdapter (result, this);

				// Set the default face ID to the ID of first face, if one or more faces are detected.
				if (faceListAdapter.faces.Count != 0)
				{

					mFaceId = faceListAdapter.faces [0].FaceId;

					// Show the thumbnail of the default face.
					ImageView imageView = (ImageView) FindViewById (Resource.Id.image_0);
					imageView.SetImageBitmap (faceListAdapter.faceThumbnails [0]);

					RefreshVerifyButtonEnabledStatus ();
				}

				// Show the list of detected face thumbnails.
				ListView listView = (ListView) FindViewById (Resource.Id.list_faces_0);
				listView.Adapter = faceListAdapter;
				listView.Visibility = ViewStates.Visible;

				// Set the face list adapters and bitmaps.
				mFaceListAdapter = faceListAdapter;
				mBitmap = null;
			}

			if (result != null && result.Length == 0)
			{
				SetInfo ("No face detected!");
			}

			mProgressDialog.Dismiss ();

			if (mFaceId != null && mPersonGroupId != null)
			{
				SetVerifyButtonEnabledStatus (true);
			}
		}

		private void Detect ()
		{
			ExecuteDetection ();
			SetSelectImageButtonEnabledStatus (false);
		}

		private async void ExecuteDetection ()
		{
			Face.Droid.Contract.Face [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Detecting in image");

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					mBitmap.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);

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
				 SetUiAfterDetection (faces, mSucceed);
			 });
		}

		private void SetInfo (String info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			textView.Text = info;
		}

		// Add a log item.
		private void AddLog (String _log)
		{
			LogHelper.AddVerificationLog (_log);
		}

		private void Manage_Persons_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (PersonGroupListActivity));
			StartActivity (intent);

			if (mFaceId != null && mPersonId != null)
			{
				SetVerifyButtonEnabledStatus (true);
			}
			else
			{
				SetVerifyButtonEnabledStatus (false);
			}
		}

		private class FaceListAdapter : BaseAdapter
		{
			public List<Face.Droid.Contract.Face> faces;
			public List<Bitmap> faceThumbnails;
			private PersonVerificationActivity activity;

			public FaceListAdapter (Face.Droid.Contract.Face [] detectionResult, PersonVerificationActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				activity = act;

				if (detectionResult != null)
				{
					faces = detectionResult.ToList ();

					foreach (Face.Droid.Contract.Face face in faces)
					{
						try
						{
							faceThumbnails.Add (ImageHelper.GenerateFaceThumbnail (activity.mBitmap, face.FaceRectangle));
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
					LayoutInflater layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face, parent, false);
				}
				convertView.Id = position;

				Bitmap thumbnailToShow = faceThumbnails [position];
				if (faces [position].FaceId.Equals (activity.mFaceId))
				{
					thumbnailToShow = ImageHelper.HighlightSelectedFaceThumbnail (thumbnailToShow);
				}

				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageBitmap (thumbnailToShow);

				return convertView;
			}
		}

		private class PersonListAdapter : BaseAdapter
		{
			public List<String> personIdList;
			public List<String> personGroupIds;
			private PersonVerificationActivity activity;

			public PersonListAdapter (PersonVerificationActivity act)
			{
				personIdList = new List<String> ();
				personGroupIds = new List<String> ();
				activity = act;

				ICollection<String> personGroups = StorageHelper.GetAllPersonGroupIds (activity);

				int index = 0;

				foreach (String personGroupId in personGroups)
				{
					personIdList.AddRange (StorageHelper.GetAllPersonIds (personGroupId, activity));

					for (int i = index; i < personIdList.Count; ++i)
					{
						personGroupIds.Add (personGroupId);
					}

					index = personIdList.Count;
				}
			}

			public override int Count
			{
				get
				{
					return personIdList.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return new String [] { personIdList [position], personGroupIds [position] };
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_person_group, parent, false);
				}
				convertView.Id = position;

				String personName = StorageHelper.GetPersonName (
				   personIdList [position], personGroupIds [position], activity);
				String personGroupName = StorageHelper.GetPersonGroupName (personGroupIds [position], activity);
				((TextView) convertView.FindViewById (Resource.Id.text_person_group)).Text = String.Format ("{0} - {1}", personGroupName, personName);

				if (position == 0)
				{
					((TextView) convertView.FindViewById (Resource.Id.text_person_group)).SetTextColor (Color.ParseColor ("#3399FF"));
				}

				return convertView;
			}
		}
	}
}