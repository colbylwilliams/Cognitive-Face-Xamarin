using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Extensions;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Model;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/find_similar_faces",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FindSimilarFaceActivity : AppCompatActivity, AdapterView.IOnItemClickListener
	{
		const int REQUEST_ADD_FACE = 0;
		const int REQUEST_SELECT_IMAGE = 1;

		FaceImageListAdapter faceListAdapter;
		FaceImageListAdapter targetFaceListAdapter;
		SimilarFaceListAdapter similarFaceListAdapter;
		ProgressDialog progressDialog;
		Button add_faces, select_image, find_similar_faces, view_log;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_find_similar_face);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			add_faces = FindViewById<Button> (Resource.Id.add_faces);
			select_image = FindViewById<Button> (Resource.Id.select_image);
			find_similar_faces = FindViewById<Button> (Resource.Id.find_similar_faces);
			view_log = FindViewById<Button> (Resource.Id.view_log);

			SetFindSimilarFaceButtonEnabledStatus (false);

			InitializeFaceList ();
			faceListAdapter = new FaceImageListAdapter ();
			FindViewById<GridView> (Resource.Id.all_faces).Adapter = faceListAdapter;

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


		void InitializeFaceList ()
		{
			var listView = FindViewById<ListView> (Resource.Id.list_faces);
			listView.OnItemClickListener = this;
		}


		protected async override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok)
			{
				await Detect (data.Data, requestCode);
			}
		}


		async Task Detect (global::Android.Net.Uri uri, int requestCode)
		{
			try
			{
				using (var bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (uri))
				{
					if (bitmap != null)
					{
						FindViewById (Resource.Id.all_faces).Visibility = ViewStates.Visible;

						SetAllButtonsEnabledStatus (false);

						var faces = await ExecuteDetection (bitmap);

						SetAllButtonsEnabledStatus (true);

						switch (requestCode)
						{
							case REQUEST_ADD_FACE:

								if (faces?.Count > 0)
								{
									faceListAdapter.AddFaces (faces, bitmap);

									var textView = FindViewById<TextView> (Resource.Id.text_all_faces);
									textView.Text = $"Face database: {faces.Count} face{(faces.Count != 1 ? "s" : "")} in total";
								}
								break;

							case REQUEST_SELECT_IMAGE:

								targetFaceListAdapter = new FaceImageListAdapter ();
								var listView = FindViewById<ListView> (Resource.Id.list_faces);
								listView.Adapter = targetFaceListAdapter;

								if (faces?.Count > 0)
								{
									targetFaceListAdapter.AddFaces (faces, bitmap);
									// Set the default face ID to the ID of first face, if one or more faces are detected.
									targetFaceListAdapter.SetSelectedIndex (0);

									// Show the thumbnail of the default face.
									var imageView = FindViewById<ImageView> (Resource.Id.image);
									imageView.SetImageBitmap (targetFaceListAdapter.GetThumbnailForPosition (0));
								}
								break;
						}

						RefreshFindSimilarFaceButtonEnabledStatus ();
						SetInfo ("Detection is done");
					}
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("No face detected!");
			}
		}


		async Task<List<Model.Face>> ExecuteDetection (Bitmap photo)
		{
			try
			{
				AddLog ("Request: Detecting in image");
				progressDialog.Show ();
				progressDialog.SetMessage ("Detecting...");
				SetInfo ("Detecting...");

				var faces = await FaceClient.Shared.DetectFacesInPhoto (() => photo.AsJpeg ());

				AddLog ($"Response: Success. Detected {faces.Count} face(s) in image");
				SetInfo ($"{faces.Count} face{(faces.Count != 1 ? "s" : "")} detected");

				if (faces != null)
				{
					AddLog ($"Response: Success. Detected {faces.Count} Face(s) in image");
				}

				return faces;
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				throw;
			}
			finally
			{
				progressDialog.Dismiss ();
			}
		}


		void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			add_faces.Enabled = isEnabled;
			select_image.Enabled = isEnabled;
			find_similar_faces.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void SetFindSimilarFaceButtonEnabledStatus (bool isEnabled)
		{
			find_similar_faces.Enabled = isEnabled;
		}


		void RefreshFindSimilarFaceButtonEnabledStatus ()
		{
			if (faceListAdapter.Count != 0 && targetFaceListAdapter?.SelectedFace != null)
			{
				SetFindSimilarFaceButtonEnabledStatus (true);
			}
			else
			{
				SetFindSimilarFaceButtonEnabledStatus (false);
			}
		}


		void Add_Faces_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_ADD_FACE);
		}


		void Select_Image_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}


		public void OnItemClick (AdapterView parent, View view, int position, long id)
		{
			if (targetFaceListAdapter.SelectedFace != targetFaceListAdapter [position])
			{
				targetFaceListAdapter.SetSelectedIndex (position);

				var imageView = FindViewById<ImageView> (Resource.Id.image);
				imageView.SetImageBitmap (targetFaceListAdapter.GetThumbnailForPosition (position));

				// Clear the result of finding similar faces.
				var similarFaces = FindViewById<GridView> (Resource.Id.similar_faces);
				similarFaceListAdapter = new SimilarFaceListAdapter (null, null);
				similarFaces.Adapter = similarFaceListAdapter;

				similarFaces = FindViewById<GridView> (Resource.Id.facial_similar_faces);
				similarFaceListAdapter = new SimilarFaceListAdapter (null, null);
				similarFaces.Adapter = similarFaceListAdapter;

				SetInfo ("");
			}
		}


		async void Find_Similar_Faces_Click (object sender, EventArgs e)
		{
			if (targetFaceListAdapter?.SelectedFace == null || faceListAdapter.Count == 0)
			{
				SetInfo ("Parameters are not ready");
				return;
			}

			SetAllButtonsEnabledStatus (false);

			await ExecuteFindSimilarFaces (FindSimilarMatchMode.MatchPerson, Resource.Id.similar_faces);
			await ExecuteFindSimilarFaces (FindSimilarMatchMode.MatchFace, Resource.Id.facial_similar_faces);

			SetAllButtonsEnabledStatus (true);
		}


		async Task ExecuteFindSimilarFaces (FindSimilarMatchMode matchMode, int gridViewResourceId)
		{
			List<SimilarFaceResult> results = null;

			try
			{
				AddLog ($"Request: Find {matchMode.ToString ()} similar faces to {targetFaceListAdapter.SelectedFace.Id} in {faceListAdapter.Count} face(s)");
				progressDialog.Show ();
				progressDialog.SetMessage ("Finding Similar Faces...");
				SetInfo ("Finding Similar Faces...");

				results = await FaceClient.Shared.FindSimilar (targetFaceListAdapter.SelectedFace, faceListAdapter.Faces, 4 /*max candidates*/, matchMode);

				var resultString = $"Found {results?.Count ?? 0} {matchMode} similar face{(results?.Count != 1 ? "s" : "")}";
				AddLog ("Response: Success. " + resultString);
				AppendInfo (resultString);
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();

			// Show the result of face finding similar faces.
			var similarFaces = FindViewById<GridView> (gridViewResourceId);
			similarFaceListAdapter = new SimilarFaceListAdapter (results, faceListAdapter);
			similarFaces.Adapter = similarFaceListAdapter;
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (FindSimilarFaceLogActivity));
			StartActivity (intent);
		}


		void SetInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		void AppendInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text += ',' + info;
		}


		void AddLog (string _log)
		{
			LogHelper.AddFindSimilarFaceLog (_log);
		}


		class SimilarFaceListAdapter : BaseAdapter<SimilarFaceResult>
		{
			readonly List<SimilarFaceResult> similarFaceResults;
			readonly FaceImageListAdapter faceListAdapter;

			public SimilarFaceListAdapter (List<SimilarFaceResult> similarFaceResults, FaceImageListAdapter faceListAdapter)
			{
				this.similarFaceResults = similarFaceResults;
				this.faceListAdapter = faceListAdapter;
			}


			public override int Count => similarFaceResults?.Count ?? 0;


			public override SimilarFaceResult this [int position] => similarFaceResults [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face, parent, false);
				}

				convertView.Id = position;

				var faceResult = similarFaceResults [position];
				var thumbnail = faceListAdapter.GetThumbnailForFace (faceResult.Face);

				convertView.FindViewById<ImageView> (Resource.Id.image_face).SetImageBitmap (thumbnail);

				return convertView;
			}
		}
	}
}