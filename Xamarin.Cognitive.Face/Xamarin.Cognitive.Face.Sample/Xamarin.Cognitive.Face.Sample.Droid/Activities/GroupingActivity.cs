using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/grouping",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class GroupingActivity : AppCompatActivity
	{
		const int REQUEST_SELECT_IMAGE = 0;

		Bitmap bitmap;
		FaceImageListAdapter faceListAdapter;
		ProgressDialog progressDialog;
		Button add_faces, group, view_log;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_grouping);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			add_faces = FindViewById<Button> (Resource.Id.add_faces);
			group = FindViewById<Button> (Resource.Id.group);
			view_log = FindViewById<Button> (Resource.Id.view_log);

			faceListAdapter = new FaceImageListAdapter ();
			FindViewById<GridView> (Resource.Id.all_faces).Adapter = faceListAdapter;

			SetGroupButtonEnabledStatus (false);

			LogHelper.ClearLog (LogType.Grouping);
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


		protected async override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok && requestCode == REQUEST_SELECT_IMAGE)
			{
				bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

				if (bitmap != null)
				{
					FindViewById (Resource.Id.all_faces).Visibility = ViewStates.Visible;

					var groupedFaces = FindViewById<ListView> (Resource.Id.grouped_faces);
					var faceGroupsAdapter = new FaceGroupsAdapter (null, null);
					groupedFaces.Adapter = faceGroupsAdapter;

					SetAllButtonsEnabledStatus (false);

					await ExecuteDetection ();
				}
			}
		}


		async Task ExecuteDetection ()
		{
			try
			{
				AddLog ("Request: Detecting in image");
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

				AddLog ($"Response: Success. Detected {faces.Count} face(s) in image");
				SetInfo ($"{faces.Count} face{(faces.Count != 1 ? "s" : "")} detected");

				// Set the default face ID to the ID of first face, if one or more faces are detected.
				if (faces?.Count > 0)
				{
					SetInfo ("Detection is done");

					faceListAdapter.AddFaces (faces, bitmap);

					var textView = FindViewById<TextView> (Resource.Id.text_all_faces);
					textView.Text = $"{faceListAdapter.Count} face{(faceListAdapter.Count != 1 ? "s" : "")} in total";
				}
				else
				{
					SetInfo ("No face detected!");
				}

				if (faceListAdapter.Count >= 2 && faceListAdapter.Count <= 100) //upper api limit?
				{
					SetGroupButtonEnabledStatus (true);
				}
				else
				{
					SetGroupButtonEnabledStatus (false);
				}

				bitmap.Dispose ();
				bitmap = null;
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("No face detected!");
			}

			progressDialog.Dismiss ();
			SetAllButtonsEnabledStatus (true);
		}


		void Add_Faces_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}


		async void Group_Click (object sender, EventArgs e)
		{
			if (faceListAdapter.Faces.Count > 0)
			{
				await ExecuteGrouping ();
			}
			else
			{
				SetInfo (Application.Context.GetString (Resource.String.no_face_to_group));
			}
		}


		async Task ExecuteGrouping ()
		{
			var faces = faceListAdapter.Faces;

			try
			{
				AddLog ($"Request: Grouping {faces.Count} face(s)");
				progressDialog.Show ();
				progressDialog.SetMessage ("Grouping...");
				SetInfo ("Grouping...");

				var result = await FaceClient.Shared.GroupFaces (faces);

				if (result != null)
				{
					AddLog ($"Response: Success. Grouped into {result.Groups.Count} face group(s).");
					SetInfo ("Grouping is done");
					SetGroupButtonEnabledStatus (false);

					// Show the result of face grouping.
					var groupedFaces = FindViewById<ListView> (Resource.Id.grouped_faces);
					var faceGroupsAdapter = new FaceGroupsAdapter (result, faceListAdapter);
					groupedFaces.Adapter = faceGroupsAdapter;
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error: Grouping was not successful.");
			}

			progressDialog.Dismiss ();
			SetAllButtonsEnabledStatus (true);
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (GroupingLogActivity));
			StartActivity (intent);
		}


		void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			var selectImageButton = FindViewById<Button> (Resource.Id.add_faces);
			selectImageButton.Enabled = isEnabled;

			group.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void SetGroupButtonEnabledStatus (bool isEnabled)
		{
			group.Enabled = isEnabled;
		}


		void SetInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		void AddLog (string log)
		{
			LogHelper.AddLog (LogType.Grouping, log);
		}


		class FaceGroupsAdapter : BaseAdapter<FaceGroup>
		{
			readonly List<FaceGroup> faceGroups;
			readonly FaceImageListAdapter faceListAdapter;

			public FaceGroupsAdapter (GroupResult result, FaceImageListAdapter faceListAdapter)
			{
				this.faceListAdapter = faceListAdapter;

				if (result != null)
				{
					faceGroups = result.Groups;

					if (result.MessyGroup != null)
					{
						faceGroups.Add (result.MessyGroup);
					}
				}
			}


			public override int Count => faceGroups?.Count ?? 0;


			public override FaceGroup this [int position] => faceGroups [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_group, parent, false);
				}

				convertView.Id = position;

				var faceGroup = faceGroups [position];
				var faceGroupName = $"{faceGroup.Title}: {faceGroup.Faces.Count} face(s)";

				convertView.FindViewById<TextView> (Resource.Id.face_group_name).Text = faceGroupName;

				var thumbnails = faceListAdapter.GetThumbnailsForFaceList (faceGroup.Faces);

				var facesAdapter = new FaceImageListAdapter (faceGroup.Faces, thumbnails);
				var gridView = convertView.FindViewById<EmbeddedGridView> (Resource.Id.faces);
				gridView.Adapter = facesAdapter;

				return convertView;
			}
		}
	}
}