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
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/find_similar_faces",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FindSimilarFaceActivity : AppCompatActivity
	{
		private Bitmap mBitmap;
		private Bitmap mTargetBitmap;
		private FaceListAdapter mFaceListAdapter;
		private FaceListAdapter mTargetFaceListAdapter;
		private SimilarFaceListAdapter mSimilarFaceListAdapter;
		private const int REQUEST_ADD_FACE = 0;
		private const int REQUEST_SELECT_IMAGE = 1;
		private UUID mFaceId;
		private ProgressDialog mProgressDialog;
		private Button add_faces, select_image, find_similar_faces, view_log = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_find_similar_face);

			mFaceListAdapter = new FaceListAdapter (this);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			add_faces = (Button) FindViewById (Resource.Id.add_faces);
			select_image = (Button) FindViewById (Resource.Id.select_image);
			find_similar_faces = (Button) FindViewById (Resource.Id.find_similar_faces);
			view_log = (Button) FindViewById (Resource.Id.view_log);

			SetFindSimilarFaceButtonEnabledStatus (false);

			InitializeFaceList ();

			LogHelper.ClearFindSimilarFaceLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			add_faces.Click += Add_Faces_Click;
			select_image.Click += Select_Image_Click;
			find_similar_faces.Click += Find_Similar_Faces_Click;
			view_log.Click += View_Log_Click;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			add_faces.Click -= Add_Faces_Click;
			select_image.Click -= Select_Image_Click;
			find_similar_faces.Click -= Find_Similar_Faces_Click;
			view_log.Click -= View_Log_Click;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode == REQUEST_ADD_FACE)
			{
				if (resultCode == Result.Ok)
				{
					mBitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

					if (mBitmap != null)
					{
						View originalFaces = FindViewById (Resource.Id.all_faces);
						originalFaces.Visibility = ViewStates.Visible;

						SetAllButtonsEnabledStatus (false);

						ExecuteDetection (REQUEST_ADD_FACE, data.Data.ToString (), mBitmap);
					}
				}
			}
			else if (requestCode == REQUEST_SELECT_IMAGE)
			{
				if (resultCode == Result.Ok)
				{
					mTargetBitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

					if (mTargetBitmap != null)
					{
						View originalFaces = FindViewById (Resource.Id.all_faces);
						originalFaces.Visibility = ViewStates.Visible;

						SetAllButtonsEnabledStatus (false);

						ExecuteDetection (REQUEST_SELECT_IMAGE, data.Data.ToString (), mTargetBitmap);
					}
				}
			}
		}

		private async void ExecuteDetection (int mRequestCode, string mImageUri, Bitmap mInternalBitmap)
		{
			Face.Droid.Contract.Face [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Detecting in image " + mImageUri);

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					mInternalBitmap.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);

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
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (mSucceed)
				 {
					 AddLog ("Response: Success. Detected " + faces.Length + " Face(s) in image");
				 }
				 if (mRequestCode == REQUEST_ADD_FACE)
				 {
					 SetUiAfterDetectionForAddFace (faces);
				 }
				 else if (mRequestCode == REQUEST_SELECT_IMAGE)
				 {
					 SetUiAfterDetectionForSelectImage (faces);
				 }
			 });
		}

		private void SetUiAfterFindPersonalSimilarFaces (Face.Droid.Contract.SimilarFace [] result)
		{
			mProgressDialog.Dismiss ();

			SetAllButtonsEnabledStatus (true);

			// Show the result of face finding similar faces.
			GridView similarFaces = (GridView) FindViewById (Resource.Id.similar_faces);
			mSimilarFaceListAdapter = new SimilarFaceListAdapter (result, this);
			similarFaces.Adapter = mSimilarFaceListAdapter;
		}

		private void SetUiAfterFindFacialSimilarFaces (Face.Droid.Contract.SimilarFace [] result)
		{
			mProgressDialog.Dismiss ();

			SetAllButtonsEnabledStatus (true);

			// Show the result of face finding similar faces.
			GridView similarFaces = (GridView) FindViewById (Resource.Id.facial_similar_faces);
			mSimilarFaceListAdapter = new SimilarFaceListAdapter (result, this);
			similarFaces.Adapter = mSimilarFaceListAdapter;
		}

		private void SetUiAfterDetectionForAddFace (Face.Droid.Contract.Face [] result)
		{
			SetAllButtonsEnabledStatus (true);

			// Show the detailed list of original faces.
			mFaceListAdapter.AddFaces (result, mBitmap);

			GridView listView = (GridView) FindViewById (Resource.Id.all_faces);
			listView.Adapter = mFaceListAdapter;

			TextView textView = (TextView) FindViewById (Resource.Id.text_all_faces);
			textView.Text = string.Format (
				"Face database: {0} face{1} in total",
					mFaceListAdapter.faces.Count,
					mFaceListAdapter.faces.Count != 1 ? "s" : "");

			RefreshFindSimilarFaceButtonEnabledStatus ();

			mBitmap = null;

			// Set the status bar.
			SetDetectionStatus ();
		}

		private void SetUiAfterDetectionForSelectImage (Face.Droid.Contract.Face [] result)
		{
			SetAllButtonsEnabledStatus (true);

			// Show the detailed list of detected faces.
			mTargetFaceListAdapter = new FaceListAdapter (this);
			mTargetFaceListAdapter.AddFaces (result, mTargetBitmap);

			// Show the list of detected face thumbnails.
			ListView listView = (ListView) FindViewById (Resource.Id.list_faces);
			listView.Adapter = mTargetFaceListAdapter;

			// Set the default face ID to the ID of first face, if one or more faces are detected.
			if (mTargetFaceListAdapter.faces.Count != 0)
			{
				mFaceId = mTargetFaceListAdapter.faces [0].FaceId;
				// Show the thumbnail of the default face.
				ImageView imageView = (ImageView) FindViewById (Resource.Id.image);
				imageView.SetImageBitmap (mTargetFaceListAdapter.faceThumbnails [0]);
			}

			RefreshFindSimilarFaceButtonEnabledStatus ();

			mTargetBitmap = null;

			// Set the status bar.
			SetDetectionStatus ();
		}

		private void SetDetectionStatus ()
		{
			if (mBitmap == null && mTargetBitmap == null)
			{
				mProgressDialog.Dismiss ();
				SetInfo ("Detection is done");
			}
			else
			{
				mProgressDialog.SetMessage ("Detecting...");
				SetInfo ("Detecting...");
			}
		}

		private void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			Button addFaceButton = (Button) FindViewById (Resource.Id.add_faces);
			addFaceButton.Enabled = isEnabled;

			Button selectImageButton = (Button) FindViewById (Resource.Id.select_image);
			selectImageButton.Enabled = isEnabled;

			Button detectButton = (Button) FindViewById (Resource.Id.find_similar_faces);
			detectButton.Enabled = isEnabled;

			Button logButton = (Button) FindViewById (Resource.Id.view_log);
			logButton.Enabled = isEnabled;
		}

		private void SetFindSimilarFaceButtonEnabledStatus (bool isEnabled)
		{
			Button button = (Button) FindViewById (Resource.Id.find_similar_faces);
			button.Enabled = isEnabled;
		}

		private void RefreshFindSimilarFaceButtonEnabledStatus ()
		{
			if (mFaceListAdapter.faces.Count != 0 && mFaceId != null)
			{
				SetFindSimilarFaceButtonEnabledStatus (true);
			}
			else
			{
				SetFindSimilarFaceButtonEnabledStatus (false);
			}
		}

		private void InitializeFaceList ()
		{
			ListView listView = (ListView) FindViewById (Resource.Id.list_faces);
			listView.OnItemClickListener = new SetOnItemClickListener (this);
		}

		private void Add_Faces_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_ADD_FACE);
		}

		private void Select_Image_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}

		private void Find_Similar_Faces_Click (object sender, EventArgs e)
		{
			if (mFaceId == null || mFaceListAdapter.faces.Count == 0)
			{
				SetInfo ("Parameters are not ready");
			}
			List<UUID> faceIds = new List<UUID> ();
			faceIds.Add (mFaceId);
			foreach (Face.Droid.Contract.Face face in mFaceListAdapter.faces)
			{
				faceIds.Add (face.FaceId);
			}

			SetAllButtonsEnabledStatus (false);
			ExecuteFindPersonalSimilarFace (faceIds.ToArray ());
			ExecuteFindFacialSimilarFace (faceIds.ToArray ());
		}

		private UUID [] CopyOfRange (UUID [] src, int start, int end)
		{
			int len = end - start;
			UUID [] dest = new UUID [len];
			Array.Copy (src, start, dest, 0, len);
			return dest;
		}

		private async void ExecuteFindPersonalSimilarFace (UUID [] mFaceIds)
		{
			Face.Droid.Contract.SimilarFace [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Find matchPerson similar faces to " + mFaceIds [0].ToString () +
					" in " + (mFaceIds.Length - 1) + " face(s)");

			try
			{
				mProgressDialog.SetMessage ("Finding Similar Faces...");
				SetInfo ("Finding Similar Faces...");

				UUID [] faceIds = CopyOfRange (mFaceIds, 1, mFaceIds.Length);

				faces = await FaceClient.Shared.FindSimilar (
						mFaceIds [0],  /* The target face ID */
						faceIds,    /*candidate faces */
						4 /*max number of candidate returned*/
						);
			}
			catch (Java.Lang.Exception e)
			{
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (mSucceed)
				 {
					 String resultString = "Found "
						 + (faces == null ? "0" : faces.Length.ToString ())
							 + " matchPerson similar face" + ((faces != null && faces.Length != 1) ? "s" : "");
					 AddLog ("Response: Success. " + resultString);
					 SetInfo (resultString);
				 }

				 // Show the result on screen when verification is done.
				 SetUiAfterFindPersonalSimilarFaces (faces);
			 });
		}

		private async void ExecuteFindFacialSimilarFace (UUID [] mFaceIds)
		{
			Face.Droid.Contract.SimilarFace [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Find matchPerson similar faces to " + mFaceIds [0].ToString () +
					" in " + (mFaceIds.Length - 1) + " face(s)");

			try
			{
				mProgressDialog.SetMessage ("Finding Similar Faces...");
				SetInfo ("Finding Similar Faces...");

				UUID [] faceIds = CopyOfRange (mFaceIds, 1, mFaceIds.Length);

				faces = await FaceClient.Shared.FindSimilar (
						mFaceIds [0],  /* The target face ID */
						faceIds,    /*candidate faces */
						4, /*max number of candidate returned*/
						Xamarin.Cognitive.Face.Droid.FaceServiceClientFindSimilarMatchMode.MatchFace
						);
			}
			catch (Java.Lang.Exception e)
			{
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (mSucceed)
				 {
					 string resultString = "Found "
							 + (faces == null ? "0" : faces.Length.ToString ())
							 + " matchFace similar face" + ((faces != null && faces.Length != 1) ? "s" : "");
					 AddLog ("Response: Success. " + resultString);
					 AppendInfo ((faces == null ? "0" : faces.Length.ToString ())
							 + " matchFace similar face" + ((faces != null && faces.Length != 1) ? "s" : ""));
				 }

				 // Show the result on screen when verification is done.
				 SetUiAfterFindFacialSimilarFaces (faces);
			 });
		}

		private void View_Log_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (FindSimilarFaceLogActivity));
			StartActivity (intent);
		}

		private void SetInfo (string info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			textView.Text = info;
		}

		private void AppendInfo (string info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			string str = (string) textView.Text;
			textView.Text = str + ',' + info;
		}

		private void AddLog (string _log)
		{
			LogHelper.AddFindSimilarFaceLog (_log);
		}

		private class SetOnItemClickListener : Java.Lang.Object, ListView.IOnItemClickListener
		{
			private FindSimilarFaceActivity activity;
			public SetOnItemClickListener (FindSimilarFaceActivity act)
			{
				activity = act;
			}

			public void OnItemClick (AdapterView parent, View view, int position, long id)
			{
				FaceListAdapter faceListAdapter = activity.mTargetFaceListAdapter;

				if (!faceListAdapter.faces [position].FaceId.Equals (activity.mFaceId))
				{
					activity.mFaceId = faceListAdapter.faces [position].FaceId;

					ImageView imageView = (ImageView) activity.FindViewById (Resource.Id.image);
					imageView.SetImageBitmap (faceListAdapter.faceThumbnails [position]);

					// Clear the result of finding similar faces.
					GridView similarFaces = (GridView) activity.FindViewById (Resource.Id.similar_faces);
					activity.mSimilarFaceListAdapter = new SimilarFaceListAdapter (null, activity);
					similarFaces.Adapter = activity.mSimilarFaceListAdapter;

					similarFaces = (GridView) activity.FindViewById (Resource.Id.facial_similar_faces);
					activity.mSimilarFaceListAdapter = new SimilarFaceListAdapter (null, activity);
					similarFaces.Adapter = activity.mSimilarFaceListAdapter;

					activity.SetInfo ("");
				}

				// Show the list of detected face thumbnails.
				ListView listView = (ListView) activity.FindViewById (Resource.Id.list_faces);
				listView.Adapter = faceListAdapter;
			}
		}

		private class FaceListAdapter : BaseAdapter
		{
			public List<Face.Droid.Contract.Face> faces;
			public List<Bitmap> faceThumbnails;
			public Dictionary<UUID, Bitmap> faceIdThumbnailMap;
			private FindSimilarFaceActivity activity;

			public FaceListAdapter (FindSimilarFaceActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				faceIdThumbnailMap = new Dictionary<UUID, Bitmap> ();
				activity = act;
			}

			public void AddFaces (Face.Droid.Contract.Face [] detectionResult, Bitmap mBitmap)
			{
				if (detectionResult != null)
				{
					List<Face.Droid.Contract.Face> detectedFaces = detectionResult.ToList ();
					foreach (Face.Droid.Contract.Face face in detectedFaces)
					{
						faces.Add (face);
						try
						{
							Bitmap faceThumbnail = ImageHelper.GenerateFaceThumbnail (mBitmap, face.FaceRectangle);
							faceThumbnails.Add (faceThumbnail);
							faceIdThumbnailMap.Add (face.FaceId, faceThumbnail);
						}
						catch (Java.IO.IOException e)
						{
							// Show the exception when generating face thumbnail fails.
							TextView textView = (TextView) activity.FindViewById (Resource.Id.info);
							textView.Text = e.Message;
						}
					}
				}
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
				if (faces [position].FaceId.Equals (activity.mFaceId))
				{
					thumbnailToShow = ImageHelper.HighlightSelectedFaceThumbnail (thumbnailToShow);
				}

				// Show the face thumbnail.
				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageBitmap (thumbnailToShow);

				return convertView;
			}
		}

		private class SimilarFaceListAdapter : BaseAdapter
		{
			private List<Face.Droid.Contract.SimilarFace> similarFaces;
			private FindSimilarFaceActivity activity;

			public SimilarFaceListAdapter (Face.Droid.Contract.SimilarFace [] findSimilarFaceResult, FindSimilarFaceActivity act)
			{
				if (findSimilarFaceResult != null)
				{
					similarFaces = findSimilarFaceResult.ToList ();
				}
				else
				{
					similarFaces = new List<Face.Droid.Contract.SimilarFace> ();
				}
				activity = act;
			}

			public override int Count
			{
				get
				{
					return similarFaces.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return similarFaces [position];
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

				// Show the face thumbnail.
				Bitmap output = null;
				foreach (KeyValuePair<UUID, Bitmap> val in activity.mFaceListAdapter.faceIdThumbnailMap)
				{
					if (val.Key.ToString () == similarFaces [position].FaceId.ToString ())
					{
						output = val.Value;
						break;
					}
				}
				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageBitmap (output);

				return convertView;
			}
		}
	}
}
