using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Util;
using Java.Text;
using Xamarin.Cognitive.Face.Droid.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/identification",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class IdentificationActivity : AppCompatActivity
	{
		private string mPersonGroupId = null;
		private bool detected = false;
		private FaceListAdapter mFaceListAdapter = null;
		private PersonGroupListAdapter mPersonGroupListAdapter = null;
		private const int REQUEST_SELECT_IMAGE = 0;
		private Bitmap mBitmap = null;
		private ProgressDialog mProgressDialog = null;
		private Button select_image, manage_person_groups, identify, view_log = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_identification);

			detected = false;

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			select_image = (Button) FindViewById (Resource.Id.select_image);
			manage_person_groups = (Button) FindViewById (Resource.Id.manage_person_groups);
			identify = (Button) FindViewById (Resource.Id.identify);
			view_log = (Button) FindViewById (Resource.Id.view_log);

			LogHelper.ClearIdentificationLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			ListView listView = (ListView) FindViewById (Resource.Id.list_person_groups_identify);
			mPersonGroupListAdapter = new PersonGroupListAdapter (this);
			listView.Adapter = mPersonGroupListAdapter;
			listView.OnItemClickListener = new SetOnItemClickListener (this);

			if (mPersonGroupListAdapter.personGroupIdList.Count != 0)
			{

				SetPersonGroupSelected (0);
			}
			else
			{

				SetPersonGroupSelected (-1);
			}

			select_image.Click += Select_Image_Click;
			manage_person_groups.Click += Manage_Person_Groups_Click;
			identify.Click += Identify_Click;
			view_log.Click += View_Log_Click;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			select_image.Click -= Select_Image_Click;
			manage_person_groups.Click -= Manage_Person_Groups_Click;
			identify.Click -= Identify_Click;
			view_log.Click -= View_Log_Click;
		}

		private void Select_Image_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}

		private void Identify_Click (object sender, EventArgs e)
		{
			// Start detection task only if the image to detect is selected.
			if (detected && mPersonGroupId != null)
			{
				// Start a background task to identify faces in the image.
				List<UUID> faceIds = new List<UUID> ();
				foreach (Face.Droid.Contract.Face face in mFaceListAdapter.faces)
				{
					faceIds.Add (face.FaceId);
				}

				SetAllButtonsEnabledStatus (false);

				ExecuteIdentification (faceIds.ToArray ());
			}
			else
			{
				// Not detected or person group exists.
				SetInfo ("Please select an image and create a person group first.");
			}
		}

		private void Manage_Person_Groups_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (PersonGroupListActivity));
			StartActivity (intent);
			RefreshIdentifyButtonEnabledStatus ();
		}

		private void View_Log_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (IdentificationLogActivity));
			StartActivity (intent);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode)
			{
				case REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						detected = false;

						// If image is selected successfully, set the image URI and bitmap.
						global::Android.Net.Uri imageUri = data.Data;
						mBitmap = ImageHelper.LoadSizeLimitedBitmapFromUri (
								imageUri, this.ContentResolver);
						if (mBitmap != null)
						{
							// Show the image on screen.
							ImageView imageView = (ImageView) FindViewById (Resource.Id.image);
							imageView.SetImageBitmap (mBitmap);
						}

						// Clear the identification result.
						FaceListAdapter faceListAdapter = new FaceListAdapter (null, this);
						ListView listView = (ListView) FindViewById (Resource.Id.list_identified_faces);
						listView.Adapter = faceListAdapter;

						// Clear the information panel.
						SetInfo ("");

						// Start detecting in image.
						Detect ();
					}
					break;
				default:
					break;
			}
		}

		private void Detect ()
		{
			SetAllButtonsEnabledStatus (false);

			// Start a background task to detect faces in the image.
			ExecuteDetection ();
		}

		private void AddLog (String _log)
		{
			LogHelper.AddIdentificationLog (_log);
		}

		// Set whether the buttons are enabled.
		private void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			Button selectImageButton = (Button) FindViewById (Resource.Id.manage_person_groups);
			selectImageButton.Enabled = isEnabled;

			Button groupButton = (Button) FindViewById (Resource.Id.select_image);
			groupButton.Enabled = isEnabled;

			Button identifyButton = (Button) FindViewById (Resource.Id.identify);
			identifyButton.Enabled = isEnabled;

			Button viewLogButton = (Button) FindViewById (Resource.Id.view_log);
			viewLogButton.Enabled = isEnabled;
		}

		// Set the group button is enabled or not.
		private void SetIdentifyButtonEnabledStatus (bool isEnabled)
		{
			Button button = (Button) FindViewById (Resource.Id.identify);
			button.Enabled = isEnabled;
		}

		// Set the group button is enabled or not.
		private void RefreshIdentifyButtonEnabledStatus ()
		{
			if (detected && mPersonGroupId != null)
			{
				SetIdentifyButtonEnabledStatus (true);
			}
			else
			{
				SetIdentifyButtonEnabledStatus (false);
			}
		}

		// Set the information panel on screen.
		private void SetInfo (string info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			textView.Text = info;
		}

		private void SetPersonGroupSelected (int position)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.text_person_group_selected);
			if (position > 0)
			{
				String personGroupIdSelected = mPersonGroupListAdapter.personGroupIdList [position];
				mPersonGroupListAdapter.personGroupIdList [position] = mPersonGroupListAdapter.personGroupIdList [0];
				mPersonGroupListAdapter.personGroupIdList [0] = personGroupIdSelected;
				ListView listView = (ListView) FindViewById (Resource.Id.list_person_groups_identify);
				listView.Adapter = mPersonGroupListAdapter;
				SetPersonGroupSelected (0);
			}
			else if (position < 0)
			{
				SetIdentifyButtonEnabledStatus (false);
				textView.SetTextColor (Color.Red);
				textView.Text = Application.Context.GetString (Resource.String.no_person_group_selected_for_identification_warning);
			}
			else
			{
				mPersonGroupId = mPersonGroupListAdapter.personGroupIdList [0];
				String personGroupName = StorageHelper.GetPersonGroupName (
						mPersonGroupId, this);
				RefreshIdentifyButtonEnabledStatus ();
				textView.SetTextColor (Color.Black);
				textView.Text = String.Format ("Person group to use: {0}", personGroupName);
			}
		}

		private async void ExecuteIdentification (UUID [] mFaceIds)
		{
			Face.Droid.Contract.IdentifyResult [] result = null;
			bool mSucceed = true;
			string logString = string.Empty;

			mProgressDialog.Show ();

			logString = "Request: Identifying faces ";
			foreach (UUID faceId in mFaceIds)
			{
				logString += faceId.ToString () + ", ";
			}
			logString += " in group " + mPersonGroupId;
			AddLog (logString);

			try
			{
				mProgressDialog.SetMessage ("Getting person group status...");
				SetInfo ("Getting person group status...");

				Shared.TrainingStatus trainingStatus = await FaceClient.Shared.GetGroupTrainingStatus (
						this.mPersonGroupId);     /* personGroupId */

				if (trainingStatus.Status != Shared.TrainingStatus.TrainingStatusType.Succeeded)
				{
					mProgressDialog.SetMessage ("Person group training status is " + trainingStatus.Status);
					SetInfo ("Person group training status is " + trainingStatus.Status);

					mSucceed = false;
				}

				mProgressDialog.SetMessage ("Identifying...");
				SetInfo ("Identifying...");


				// Start identification.
				result = await FaceClient.Shared.Identity (
						mPersonGroupId,   /* personGroupId */
						mFaceIds,                  /* faceIds */
						1);  /* maxNumOfCandidatesReturned */
			}
			catch (Java.Lang.Exception e)
			{
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 mProgressDialog.Dismiss ();

				 SetAllButtonsEnabledStatus (true);
				 SetIdentifyButtonEnabledStatus (false);

				 if (mSucceed)
				 {
					 // Set the information about the detection result.
					 SetInfo ("Identification is done");

					 if (result != null)
					 {
						 mFaceListAdapter.SetIdentificationResult (result);

						 logString = "Response: Success. ";
						 foreach (Face.Droid.Contract.IdentifyResult identifyResult in result)
						 {
							 logString += "Face " + identifyResult.FaceId.ToString () + " is identified as "
									 + (identifyResult.Candidates.Count > 0
										? ((Face.Droid.Contract.Candidate) identifyResult.Candidates [0]).PersonId.ToString ()
											 : "Unknown Person")
									 + ". ";
						 }
						 AddLog (logString);

						 // Show the detailed list of detected faces.
						 ListView listView = (ListView) FindViewById (Resource.Id.list_identified_faces);
						 listView.Adapter = mFaceListAdapter;
					 }
				 }

			 });
		}

		private async void ExecuteDetection ()
		{
			Face.Droid.Contract.Face [] faces = null;

			mProgressDialog.Show ();
			//AddLog("Request: Detecting in image " + mImageUri);

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
						faces = await FaceClient.Shared.Detect (output, true, false, null);
					}
				}
			}
			catch (Java.Lang.Exception e)
			{
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 mProgressDialog.Dismiss ();

				 SetAllButtonsEnabledStatus (true);

				 if (faces != null)
				 {
					 // Set the adapter of the ListView which contains the details of detected faces.
					 mFaceListAdapter = new FaceListAdapter (faces, this);
					 ListView listView = (ListView) FindViewById (Resource.Id.list_identified_faces);
					 listView.Adapter = mFaceListAdapter;

					 if (faces.Length == 0)
					 {
						 detected = false;
						 SetInfo ("No faces detected!");
					 }
					 else
					 {
						 detected = true;
						 SetInfo ("Click on the \"Identify\" button to identify the faces in image.");
					 }
				 }
				 else
				 {
					 detected = false;
				 }

				 RefreshIdentifyButtonEnabledStatus ();

			 });
		}

		private class SetOnItemClickListener : Java.Lang.Object, ListView.IOnItemClickListener
		{
			IdentificationActivity activity;
			public SetOnItemClickListener (IdentificationActivity act)
			{
				this.activity = act;
			}


			public void OnItemClick (AdapterView parent, View view, int position, long id)
			{
				activity.SetPersonGroupSelected (position);
			}
		}

		private class FaceListAdapter : BaseAdapter
		{
			public List<Face.Droid.Contract.Face> faces;
			private List<Face.Droid.Contract.IdentifyResult> mIdentifyResults;
			private List<Bitmap> faceThumbnails;
			private IdentificationActivity activity;

			public FaceListAdapter (Face.Droid.Contract.Face [] detectionResult, IdentificationActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				mIdentifyResults = new List<Face.Droid.Contract.IdentifyResult> ();
				activity = act;

				if (detectionResult != null)
				{
					faces = detectionResult.ToList ();
					foreach (Face.Droid.Contract.Face face in faces)
					{
						try
						{
							faceThumbnails.Add (ImageHelper.GenerateFaceThumbnail (
									activity.mBitmap, face.FaceRectangle));
						}
						catch (Java.IO.IOException e)
						{
							activity.SetInfo (e.Message);
						}
					}
				}
			}

			public void SetIdentificationResult (Face.Droid.Contract.IdentifyResult [] identifyResults)
			{
				mIdentifyResults = identifyResults.ToList ();
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_description, parent, false);
				}
				convertView.Id = position;

				// Show the face thumbnail.
				((ImageView) convertView.FindViewById (Resource.Id.face_thumbnail)).SetImageBitmap (
						faceThumbnails [position]);

				if (mIdentifyResults.Count == faces.Count)
				{
					// Show the face details.
					DecimalFormat formatter = new DecimalFormat ("#0.00");
					if (mIdentifyResults [position].Candidates.Count > 0)
					{
						String personId =
								((Face.Droid.Contract.Candidate) mIdentifyResults [position].Candidates [0]).PersonId.ToString ();
						String personName = StorageHelper.GetPersonName (
								personId, activity.mPersonGroupId, activity);
						String identity = "Person: " + personName + "\n"
								+ "Confidence: " + formatter.Format (
								((Face.Droid.Contract.Candidate) mIdentifyResults [position].Candidates [0]).Confidence);
						((TextView) convertView.FindViewById (Resource.Id.text_detected_face)).Text = identity;
					}
					else
					{
						((TextView) convertView.FindViewById (Resource.Id.text_detected_face)).Text = Application.Context.GetString (Resource.String.face_cannot_be_identified);
					}
				}



				return convertView;
			}
		}

		private class PersonGroupListAdapter : BaseAdapter
		{
			public List<String> personGroupIdList;
			private IdentificationActivity activity;

			public PersonGroupListAdapter (IdentificationActivity act)
			{
				personGroupIdList = new List<String> ();
				activity = act;

				ICollection<String> personGroupIds = StorageHelper.GetAllPersonGroupIds (activity);

				foreach (String personGroupId in personGroupIds)
				{
					personGroupIdList.Add (personGroupId);
					if (activity.mPersonGroupId != null && personGroupId.Equals (activity.mPersonGroupId))
					{
						personGroupIdList [personGroupIdList.Count - 1] = activity.mPersonGroupListAdapter.personGroupIdList [0];
						activity.mPersonGroupListAdapter.personGroupIdList [0] = personGroupId;
					}
				}
			}

			public override int Count
			{
				get
				{
					return personGroupIdList.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return personGroupIdList [position];
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

				// set the text of the item
				String personGroupName = StorageHelper.GetPersonGroupName (
						personGroupIdList [position], activity);
				int personNumberInGroup = StorageHelper.GetAllPersonIds (
						personGroupIdList [position], activity).Count ();
				((TextView) convertView.FindViewById (Resource.Id.text_person_group)).Text =
						String.Format (
								"{0} (Person count: {1})",
								personGroupName,
								personNumberInGroup);

				if (position == 0)
				{
					((TextView) convertView.FindViewById (Resource.Id.text_person_group)).SetTextColor (
							Color.ParseColor ("#3399FF"));
				}

				return convertView;
			}
		}

	}
}
