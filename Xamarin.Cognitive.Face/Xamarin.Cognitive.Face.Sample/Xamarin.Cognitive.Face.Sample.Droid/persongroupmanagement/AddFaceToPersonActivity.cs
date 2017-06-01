using System;
using System.Collections.Generic;
using System.IO;
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
using Java.IO;
using Java.Util;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/add_face_to_person",
			  ParentActivity = typeof (PersonActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class AddFaceToPersonActivity : AppCompatActivity
	{
		string imageUri;
		Bitmap bitmap;
		FaceGridViewAdapter faceGridViewAdapter;
		ProgressDialog progressDialog;
		Button done_and_save;
		GridView gridView;

		PersonGroup Group => FaceState.Current.CurrentGroup;
		Person Person => FaceState.Current.CurrentPerson;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_add_face_to_person);

			Bundle bundle = Intent.Extras;

			if (bundle != null)
			{
				imageUri = bundle.GetString ("ImageUri");
			}

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			done_and_save = FindViewById<Button> (Resource.Id.done_and_save);
			gridView = FindViewById<GridView> (Resource.Id.gridView_faces_to_select);
		}


		protected override async void OnResume ()
		{
			base.OnResume ();

			done_and_save.Click += Done_And_Save_Click;

			var uri = global::Android.Net.Uri.Parse (imageUri);
			bitmap = ImageHelper.LoadSizeLimitedBitmapFromUri (uri, ContentResolver);

			if (bitmap != null)
			{
				await ExecuteDetection ();
			}
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			done_and_save.Click -= Done_And_Save_Click;
		}


		//protected override void OnSaveInstanceState (Bundle outState)
		//{
		//	base.OnSaveInstanceState (outState);
		//	outState.PutString ("PersonId", mPersonId);
		//	outState.PutString ("PersonGroupId", mPersonGroupId);
		//	outState.PutString ("ImageUriStr", mImageUriStr);
		//}

		//protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		//{
		//	base.OnRestoreInstanceState (savedInstanceState);
		//	mPersonId = savedInstanceState.GetString ("PersonId");
		//	mPersonGroupId = savedInstanceState.GetString ("PersonGroupId");
		//	mImageUriStr = savedInstanceState.GetString ("ImageUriStr");
		//}

		async Task ExecuteDetection ()
		{
			Face.Droid.Contract.Face [] faces = null;

			progressDialog.Show ();
			AddLog ("Request: Detecting " + imageUri);

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					bitmap.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);

					using (ByteArrayInputStream inputStream = new ByteArrayInputStream (pre_output.ToArray ()))
					{
						byte [] arr = new byte [inputStream.Available ()];
						inputStream.Read (arr);
						var output = new MemoryStream (arr);

						progressDialog.SetMessage ("Detecting...");
						SetInfo ("Detecting...");
						faces = await FaceClient.Shared.Detect (output, true, false, null);
					}

					var faces_count = faces?.Length ?? 0;
					AddLog ($"Response: Success. Detected {faces_count} Face(s)");

					SetUiAfterDetection (faces, true);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetUiAfterDetection (faces, false);
			}
		}


		void SetUiAfterDetection (Face.Droid.Contract.Face [] result, bool succeed)
		{
			progressDialog.Dismiss ();

			if (succeed)
			{
				if (result != null)
				{
					SetInfo (result.Count ().ToString () + " face"
							+ (result.Count () != 1 ? "s" : "") + " detected");
				}
				else
				{
					SetInfo ("0 face detected");
				}

				//faceGridViewAdapter = new FaceGridViewAdapter (result, this);
				//gridView.Adapter = faceGridViewAdapter;
			}
		}


		async void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (faceGridViewAdapter != null)
			{
				var faceIndices = new List<int> ();

				for (int i = 0; i < faceGridViewAdapter.faceRectList.Count; ++i)
				{
					if (faceGridViewAdapter.faceChecked [i])
					{
						faceIndices.Add (i);
					}
				}

				if (faceIndices.Count > 0)
				{
					await ExecuteFaceTask (faceIndices);
				}
				else
				{
					Finish ();
				}
			}
		}


		async Task ExecuteFaceTask (List<int> mFaceIndices)
		{
			Face.Droid.Contract.AddPersistedFaceResult result = null;
			//bool mSucceed = true;

			progressDialog.Show ();

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					if (!bitmap.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output))
					{

					}

					using (ByteArrayInputStream inputStream = new ByteArrayInputStream (pre_output.ToArray ()))
					{
						var bytes = new byte [inputStream.Available ()];
						inputStream.Read (bytes);
						var output = new MemoryStream (bytes);

						progressDialog.SetMessage ("Adding face...");
						SetInfo ("Adding face...");

						foreach (int index in mFaceIndices)
						{
							Face.Droid.Contract.FaceRectangle faceRect = faceGridViewAdapter.faceRectList [index];
							AddLog ($"Request: Adding face to person {Person.Id}");

							//result = await FaceClient.Shared.AddFaceForPerson (Person, Group, face, output, "User data", faceRect);

							faceGridViewAdapter.faceIdList [index] = result.PersistedFaceId;
						}
					}
				}
			}
			catch (Exception e)
			{
				//mSucceed = false;
				AddLog (e.Message);
			}

			//RunOnUiThread (() =>
			//{
			//	progressDialog.Dismiss ();

			//	if (mSucceed)
			//	{
			//		string faceIds = "";

			//		foreach (int index in mFaceIndices)
			//		{
			//			string faceId = faceGridViewAdapter.faceIdList [index].ToString ();
			//			faceIds += faceId + ", ";

			//			try
			//			{
			//				var file = System.IO.Path.Combine (Application.Context.FilesDir.Path, faceId);

			//				using (var fs = new FileStream (file, FileMode.OpenOrCreate))
			//				{
			//					faceGridViewAdapter.faceThumbnails [index].Compress (Bitmap.CompressFormat.Jpeg, 100, fs);
			//				}

			//				var uri = global::Android.Net.Uri.Parse (file);
			//				StorageHelper.SetFaceUri (faceId, uri.ToString (), mPersonId, this);
			//			}
			//			catch (Java.IO.IOException e)
			//			{
			//				SetInfo (e.Message);
			//			}
			//		}

			//		AddLog ("Response: Success. Face(s) " + faceIds + "added to person " + mPersonId);
			//		Finish ();
			//	}
			//});
		}


		void AddLog (string log)
		{
			LogHelper.AddIdentificationLog (log);
		}


		void SetInfo (string info)
		{
			TextView textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		class FaceGridViewAdapter : BaseAdapter, CompoundButton.IOnCheckedChangeListener
		{
			public List<UUID> faceIdList;
			public List<Face.Droid.Contract.FaceRectangle> faceRectList;
			public List<Bitmap> faceThumbnails;
			public List<bool> faceChecked;

			public FaceGridViewAdapter (Face.Droid.Contract.Face [] detectionResult)
			{
				faceIdList = new List<UUID> ();
				faceRectList = new List<Face.Droid.Contract.FaceRectangle> ();
				faceThumbnails = new List<Bitmap> ();
				faceChecked = new List<bool> ();
				//activity = act;

				if (detectionResult != null)
				{
					List<Face.Droid.Contract.Face> faces = detectionResult.ToList ();

					//foreach (Face.Droid.Contract.Face face in faces)
					//{
					//	try
					//	{
					//		faceThumbnails.Add (ImageHelper.GenerateFaceThumbnail (activity.mBitmap, face.FaceRectangle));

					//		faceIdList.Add (null);
					//		faceRectList.Add (face.FaceRectangle);

					//		faceChecked.Add (false);
					//	}
					//	catch (Java.IO.IOException e)
					//	{
					//		activity.SetInfo (e.Message);
					//	}
					//}
				}
			}


			public override int Count
			{
				get
				{
					return faceRectList.Count;
				}
			}


			public override Java.Lang.Object GetItem (int position)
			{
				return faceRectList [position];
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_checkbox, parent, false);
				}

				convertView.Id = position;

				convertView.FindViewById<ImageView> (Resource.Id.image_face).SetImageBitmap (faceThumbnails [position]);

				CheckBox checkBox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox_face);
				checkBox.Checked = faceChecked [position];
				checkBox.SetOnCheckedChangeListener (this);

				return convertView;
			}


			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				//adapter.faceChecked [position] = isChecked;
			}
		}
	}
}