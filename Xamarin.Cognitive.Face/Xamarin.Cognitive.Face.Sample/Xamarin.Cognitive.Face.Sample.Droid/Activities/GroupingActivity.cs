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
using Java.Util;
using Xamarin.Cognitive.Face.Droid.Contract;
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/grouping",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class GroupingActivity : AppCompatActivity
	{
		private Bitmap mBitmap = null;
		private FaceListAdapter mFaceListAdapter = null;
		private int REQUEST_SELECT_IMAGE = 0;
		private ProgressDialog mProgressDialog = null;
		private Button add_faces, group, view_log = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_grouping);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			add_faces = (Button) FindViewById (Resource.Id.add_faces);
			group = (Button) FindViewById (Resource.Id.group);
			view_log = (Button) FindViewById (Resource.Id.view_log);

			mFaceListAdapter = new FaceListAdapter (this);

			SetGroupButtonEnabledStatus (false);

			LogHelper.ClearGroupingLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			add_faces.Click += Add_Faces_Click;
			group.Click += Group_Click;
			view_log.Click += View_Log_Click;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			add_faces.Click -= Add_Faces_Click;
			group.Click -= Group_Click;
			view_log.Click -= View_Log_Click;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			if (requestCode == REQUEST_SELECT_IMAGE)
			{
				if (resultCode == Result.Ok)
				{
					mBitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

					if (mBitmap != null)
					{
						View originalFaces = FindViewById (Resource.Id.all_faces);
						originalFaces.Visibility = ViewStates.Visible;

						ListView groupedFaces = (ListView) FindViewById (Resource.Id.grouped_faces);
						FaceGroupsAdapter faceGroupsAdapter = new FaceGroupsAdapter (null, this);
						groupedFaces.Adapter = faceGroupsAdapter;

						SetAllButtonsEnabledStatus (false);

						ExecuteDetection (data.Data.ToString ());
					}
				}
			}
		}

		private async void ExecuteDetection (string mImageUri)
		{
			Face.Droid.Contract.Face [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Detecting in image " + mImageUri);

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
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (mSucceed)
				 {
					 AddLog ("Response: Success. Detected " + faces.Count () + " Face(s) in image");
				 }

				 mProgressDialog.Dismiss ();

				 SetAllButtonsEnabledStatus (true);

				 if (faces != null)
				 {
					 SetInfo ("Detection is done");

					 // Show the detailed list of original faces.
					 mFaceListAdapter.AddFaces (faces);
					 GridView listView = (GridView) FindViewById (Resource.Id.all_faces);
					 listView.Adapter = mFaceListAdapter;

					 TextView textView = (TextView) FindViewById (Resource.Id.text_all_faces);
					 textView.Text = String.Format (
							 "{0} face{1} in total",
							 mFaceListAdapter.faces.Count,
							 mFaceListAdapter.faces.Count != 1 ? "s" : "");
				 }

				 if (mFaceListAdapter.faces.Count >= 2 && mFaceListAdapter.faces.Count <= 100)
				 {
					 SetGroupButtonEnabledStatus (true);
				 }
				 else
				 {
					 SetGroupButtonEnabledStatus (false);
				 }
			 });
		}

		private void Add_Faces_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}

		private void Group_Click (object sender, EventArgs e)
		{
			List<UUID> faceIds = new List<UUID> ();

			foreach (Face.Droid.Contract.Face face in mFaceListAdapter.faces)
			{
				faceIds.Add (face.FaceId);
			}

			if (faceIds.Count > 0)
			{
				ExecuteGrouping (faceIds.ToArray ());
				SetAllButtonsEnabledStatus (false);
			}
			else
			{
				TextView textView = (TextView) FindViewById (Resource.Id.info);
				textView.Text = Application.Context.GetString (Resource.String.no_face_to_group);
			}
		}

		private async void ExecuteGrouping (UUID [] faceIds)
		{
			GroupResult result = null;

			mProgressDialog.Show ();
			AddLog ("Request: Grouping " + faceIds.Count () + " face(s)");

			try
			{
				mProgressDialog.SetMessage ("Grouping...");
				SetInfo ("Grouping...");
				result = await FaceClient.Shared.Group (faceIds);
			}
			catch (Java.Lang.Exception e)
			{
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 if (result != null)
				 {
					 AddLog ("Response: Success. Grouped into " + result.Groups.Count + " face group(s).");
				 }

				 mProgressDialog.Dismiss ();

				 SetAllButtonsEnabledStatus (true);

				 if (result != null)
				 {
					 SetInfo ("Grouping is done");
					 SetGroupButtonEnabledStatus (false);

					 // Show the result of face grouping.
					 ListView groupedFaces = (ListView) FindViewById (Resource.Id.grouped_faces);
					 FaceGroupsAdapter faceGroupsAdapter = new FaceGroupsAdapter (result, this);
					 groupedFaces.Adapter = faceGroupsAdapter;
				 }

			 });
		}

		private void View_Log_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (GroupingLogActivity));
			StartActivity (intent);
		}

		private void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			Button selectImageButton = (Button) FindViewById (Resource.Id.add_faces);
			selectImageButton.Enabled = isEnabled;

			Button groupButton = (Button) FindViewById (Resource.Id.group);
			groupButton.Enabled = isEnabled;

			Button ViewLogButton = (Button) FindViewById (Resource.Id.view_log);
			ViewLogButton.Enabled = isEnabled;
		}

		private void SetGroupButtonEnabledStatus (bool isEnabled)
		{
			Button button = (Button) FindViewById (Resource.Id.group);
			button.Enabled = isEnabled;
		}

		private void SetInfo (String info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			textView.Text = info;
		}

		private void AddLog (String _log)
		{
			LogHelper.AddGroupingLog (_log);
		}

		private class FaceListAdapter : BaseAdapter
		{
			public List<Face.Droid.Contract.Face> faces;
			private List<Bitmap> faceThumbnails;
			public Dictionary<UUID, Bitmap> faceIdThumbnailMap;
			private GroupingActivity activity;

			public FaceListAdapter (GroupingActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				faceIdThumbnailMap = new Dictionary<UUID, Bitmap> ();
				activity = act;
			}

			public void AddFaces (Face.Droid.Contract.Face [] detectionResult)
			{
				if (detectionResult != null)
				{
					List<Face.Droid.Contract.Face> detectedFaces = detectionResult.ToList ();

					foreach (Face.Droid.Contract.Face face in detectedFaces)
					{
						faces.Add (face);
						try
						{
							Bitmap faceThumbnail = ImageHelper.GenerateFaceThumbnail (activity.mBitmap, face.FaceRectangle);
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

				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageBitmap (faceThumbnails [position]);

				return convertView;
			}
		}

		private class FaceGroupsAdapter : BaseAdapter
		{
			List<List<UUID>> faceGroups;
			private GroupingActivity activity;

			public FaceGroupsAdapter (GroupResult result, GroupingActivity act)
			{
				faceGroups = new List<List<UUID>> ();
				activity = act;

				if (result != null)
				{
					foreach (Java.Lang.Object o in result.Groups)
					{
						var arr = o.ToArray<UUID> ();
						faceGroups.Add (arr.ToList ());
					}

					var messyList = result.MessyGroup.Cast<UUID> ().ToList ();

					faceGroups.Add (messyList);
				}
			}

			public override int Count
			{
				get
				{
					return faceGroups.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return null;
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_group, parent, false);
				}
				convertView.Id = position;

				String faceGroupName = "Group " + position + ": " + faceGroups [position].Count + " face(s)";
				if (position == faceGroups.Count - 1)
				{
					faceGroupName = "Messy Group: " + faceGroups [position].Count + " face(s)";
				}

				((TextView) convertView.FindViewById (Resource.Id.face_group_name)).Text = faceGroupName;

				FacesAdapter facesAdapter = new FacesAdapter (faceGroups [position], activity);
				EmbeddedGridView gridView = (EmbeddedGridView) convertView.FindViewById (Resource.Id.faces);
				gridView.Adapter = facesAdapter;

				return convertView;
			}
		}

		private class FacesAdapter : BaseAdapter
		{
			private List<UUID> faces;
			private GroupingActivity activity;

			public FacesAdapter (List<UUID> result, GroupingActivity act)
			{
				faces = new List<UUID> ();
				faces.AddRange (result);
				activity = act;
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

				// Show the face thumbnail.
				Bitmap output = null;
				foreach (KeyValuePair<UUID, Bitmap> val in activity.mFaceListAdapter.faceIdThumbnailMap)
				{
					if (val.Key.ToString () == faces [position].ToString ())
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
