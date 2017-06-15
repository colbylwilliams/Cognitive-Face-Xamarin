using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;
using System.Threading.Tasks;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/identification",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class IdentificationActivity : AppCompatActivity, AdapterView.IOnItemClickListener
	{
		const int REQUEST_SELECT_IMAGE = 0;

		bool detected;
		PersonGroup selectedPersonGroup;
		FaceListAdapter faceListAdapter;
		PersonGroupsListAdapter personGroupListAdapter;
		Bitmap bitmap;
		ProgressDialog progressDialog;
		Button select_image, manage_person_groups, identify, view_log;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_identification);

			detected = false;

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			select_image = FindViewById<Button> (Resource.Id.select_image);
			manage_person_groups = FindViewById<Button> (Resource.Id.manage_person_groups);
			identify = FindViewById<Button> (Resource.Id.identify);
			view_log = FindViewById<Button> (Resource.Id.view_log);

			LogHelper.ClearIdentificationLog ();
		}


		protected async override void OnResume ()
		{
			base.OnResume ();

			var groups = await FaceClient.Shared.GetPersonGroups ();

			var listView = FindViewById<ListView> (Resource.Id.list_person_groups_identify);
			personGroupListAdapter = new PersonGroupsListAdapter (groups);
			listView.Adapter = personGroupListAdapter;
			listView.OnItemClickListener = this;

			if (groups.Count > 0)
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


		void Select_Image_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}


		async void Identify_Click (object sender, EventArgs e)
		{
			// Start detection task only if the image to detect is selected.
			if (detected && selectedPersonGroup != null)
			{
				SetAllButtonsEnabledStatus (false);

				await ExecuteIdentification ();
			}
			else
			{
				// Not detected or person group exists.
				SetInfo ("Please select an image and create a person group first.");
			}
		}


		void Manage_Person_Groups_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (PersonGroupListActivity));
			StartActivity (intent);
			RefreshIdentifyButtonEnabledStatus ();
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (IdentificationLogActivity));
			StartActivity (intent);
		}


		protected async override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode)
			{
				case REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						detected = false;

						// If image is selected successfully, set the image URI and bitmap.
						bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

						if (bitmap != null)
						{
							// Show the image on screen.
							var imageView = FindViewById<ImageView> (Resource.Id.image);
							imageView.SetImageBitmap (bitmap);
						}

						// Clear the identification result.
						var emptyFaceListAdapter = new FaceListAdapter (null, null);
						var listView = FindViewById<ListView> (Resource.Id.list_identified_faces);
						listView.Adapter = emptyFaceListAdapter;

						// Clear the information panel.
						SetInfo ("");

						// Start detecting in image.
						await Detect ();
					}
					break;
			}
		}


		public void OnItemClick (AdapterView parent, View view, int position, long id)
		{
			SetPersonGroupSelected (position);
		}


		async Task Detect ()
		{
			SetAllButtonsEnabledStatus (false);

			// Start a background task to detect faces in the image.
			await ExecuteDetection ();
		}


		void AddLog (string log)
		{
			LogHelper.AddIdentificationLog (log);
		}


		// Set whether the buttons are enabled.
		void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			manage_person_groups.Enabled = isEnabled;
			select_image.Enabled = isEnabled;
			identify.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		// Set the group button is enabled or not.
		void SetIdentifyButtonEnabledStatus (bool isEnabled)
		{
			var button = FindViewById<Button> (Resource.Id.identify);
			button.Enabled = isEnabled;
		}


		// Set the group button is enabled or not.
		void RefreshIdentifyButtonEnabledStatus ()
		{
			if (detected && selectedPersonGroup != null)
			{
				SetIdentifyButtonEnabledStatus (true);
			}
			else
			{
				SetIdentifyButtonEnabledStatus (false);
			}
		}


		void SetInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		void SetPersonGroupSelected (int position)
		{
			personGroupListAdapter.SetSelectedPosition (position);

			var textView = FindViewById<TextView> (Resource.Id.text_person_group_selected);

			if (position >= 0)
			{
				selectedPersonGroup = personGroupListAdapter.GetGroup (position);

				RefreshIdentifyButtonEnabledStatus ();
				textView.SetTextColor (Color.Black);
				textView.Text = $"Person group to use: {selectedPersonGroup.Name}";
			}
			else if (position < 0)
			{
				SetIdentifyButtonEnabledStatus (false);
				textView.SetTextColor (Color.Red);
				textView.Text = Application.Context.GetString (Resource.String.no_person_group_selected_for_identification_warning);
			}
		}


		async Task ExecuteIdentification ()
		{
			try
			{
				progressDialog.Show ();

				var ids = string.Join (", ", faceListAdapter.DetectedFaces.Select (f => f.Id));

				AddLog ($"Request: Identifying faces {ids}");

				progressDialog.SetMessage ("Getting person group status...");
				SetInfo ("Getting person group status...");

				var trainingStatus = await FaceClient.Shared.GetGroupTrainingStatus (selectedPersonGroup);

				if (trainingStatus.Status == TrainingStatus.TrainingStatusType.Succeeded)
				{
					progressDialog.SetMessage ("Identifying...");
					SetInfo ("Identifying...");

					var results = await FaceClient.Shared.Identify (
						selectedPersonGroup,            /* personGroupId */
						faceListAdapter.DetectedFaces); /* faceIds */

					SetAllButtonsEnabledStatus (true);
					SetIdentifyButtonEnabledStatus (false);

					// Set the information about the detection result.
					SetInfo ("Identification is done");

					if (results != null)
					{
						faceListAdapter.SetIdentificationResult (results);

						foreach (var identifyResult in results)
						{
							var personString = identifyResult.CandidateResult != null
															 ? identifyResult.CandidateResult.PersonId : "Unknown Person";

							AddLog ($"Face {identifyResult.FaceId} is identified as {personString}.");
						}

						// Show the detailed list of detected faces.
						var listView = FindViewById<ListView> (Resource.Id.list_identified_faces);
						listView.Adapter = faceListAdapter;
					}
				}
				else
				{
					progressDialog.SetMessage ("Person group training status is " + trainingStatus.Status);
					SetInfo ("Person group training status is " + trainingStatus.Status);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		async Task ExecuteDetection ()
		{
			try
			{
				progressDialog.Show ();
				progressDialog.SetMessage ("Detecting...");
				SetInfo ("Detecting...");

				var faces = await FaceClient.Shared.DetectFacesInPhoto (() => bitmap.AsJpeg ());

				SetAllButtonsEnabledStatus (true);

				faceListAdapter = new FaceListAdapter (faces, bitmap);
				var listView = FindViewById<ListView> (Resource.Id.list_identified_faces);
				listView.Adapter = faceListAdapter;

				if (faces?.Count > 0)
				{
					detected = true;
					SetInfo ("Click on the \"Identify\" button to identify the faces in image.");
				}
				else
				{
					detected = false;
					SetInfo ("No faces detected!");
				}
			}
			catch (Exception e)
			{
				detected = false;
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
			RefreshIdentifyButtonEnabledStatus ();
		}


		class FaceListAdapter : BaseAdapter<Model.Face>
		{
			readonly List<Bitmap> faceThumbnails;

			List<IdentificationResult> identifyResults;

			public List<Model.Face> DetectedFaces { get; private set; }

			public FaceListAdapter (List<Model.Face> detectedFaces, Bitmap photo)
			{
				DetectedFaces = detectedFaces;

				if (detectedFaces != null && photo != null)
				{
					faceThumbnails = detectedFaces.GenerateThumbnails (photo);
				}
			}


			public void SetIdentificationResult (List<IdentificationResult> identifyResults)
			{
				this.identifyResults = identifyResults;
			}


			public override int Count => DetectedFaces?.Count ?? 0;


			public override Model.Face this [int position] => DetectedFaces [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_description, parent, false);
				}

				convertView.Id = position;

				// Show the face thumbnail.
				convertView.FindViewById<ImageView> (Resource.Id.face_thumbnail).SetImageBitmap (faceThumbnails [position]);

				if (identifyResults?.Count == DetectedFaces.Count)
				{
					// Show the face details.
					//DecimalFormat formatter = new DecimalFormat ("#0.00");
					var result = identifyResults [position];

					if (result.CandidateResults.Count > 0)
					{
						string personId = result.CandidateResult.PersonId;
						string identity = $"Person: {result.CandidateResult.Person.Name}\nConfidence: {result.CandidateResult.Confidence}";

						convertView.FindViewById<TextView> (Resource.Id.text_detected_face).Text = identity;
					}
					else
					{
						convertView.FindViewById<TextView> (Resource.Id.text_detected_face).Text = Application.Context.GetString (Resource.String.face_cannot_be_identified);
					}
				}

				return convertView;
			}
		}
	}
}