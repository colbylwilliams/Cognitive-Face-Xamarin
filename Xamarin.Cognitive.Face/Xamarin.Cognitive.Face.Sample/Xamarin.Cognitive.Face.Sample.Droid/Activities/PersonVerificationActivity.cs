using System;
using System.Collections.Generic;
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
	[Activity (Label = "@string/person_verification",
			  ParentActivity = typeof (VerificationMenuActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonVerificationActivity : AppCompatActivity
	{
		const int REQUEST_SELECT_IMAGE = 0;

		Bitmap bitmap;
		FaceImageListAdapter faceListAdapter;
		ProgressDialog progressDialog;
		PersonListAdapter personListAdapter;
		ListView listView_persons, listView_faces;
		Button select_image, manage_persons, verify, view_log;


		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_verification_person);

			listView_persons = FindViewById<ListView> (Resource.Id.list_persons);
			listView_faces = FindViewById<ListView> (Resource.Id.list_faces_0);
			select_image = FindViewById<Button> (Resource.Id.select_image_0);
			manage_persons = FindViewById<Button> (Resource.Id.manage_persons);
			verify = FindViewById<Button> (Resource.Id.verify);
			view_log = FindViewById<Button> (Resource.Id.view_log);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			ClearDetectedFaces ();
			SetVerifyButtonEnabledStatus (false);
			LogHelper.ClearVerificationLog ();
		}


		protected async override void OnResume ()
		{
			base.OnResume ();

			listView_persons.ItemClick += ListView_Persons_ItemClick;
			listView_faces.ItemClick += ListView_Faces_0_ItemClick;
			select_image.Click += Select_Image_Click;
			manage_persons.Click += Manage_Persons_Click;
			verify.Click += Verify_Click;
			view_log.Click += View_Log_Click;

			await LoadPeople ();
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			listView_persons.ItemClick -= ListView_Persons_ItemClick;
			listView_faces.ItemClick -= ListView_Faces_0_ItemClick;
			select_image.Click -= Select_Image_Click;
			manage_persons.Click -= Manage_Persons_Click;
			verify.Click -= Verify_Click;
			view_log.Click -= View_Log_Click;
		}


		async Task LoadPeople ()
		{
			try
			{
				progressDialog.Show ();
				progressDialog.SetMessage ("Loading groups and people...");

				//load all groups and people
				var groups = await FaceClient.Shared.LoadGroupsWithPeople ();

				var selectedPerson = personListAdapter?.SelectedPerson;

				personListAdapter = new PersonListAdapter (groups);
				listView_persons.Adapter = personListAdapter;

				if (personListAdapter.Count > 0)
				{
					if (selectedPerson != null)
					{
						personListAdapter.Select (selectedPerson);
					}
					else
					{
						SetPersonSelected (0);
					}
				}
				else
				{
					SetPersonSelected (-1);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		void SetPersonSelected (int position)
		{
			var textView = FindViewById<TextView> (Resource.Id.text_person_selected);

			if (position >= 0)
			{
				var person = personListAdapter [position];
				personListAdapter.SelectAt (position);

				listView_persons.SetSelectionAfterHeaderView ();

				SetVerifyButtonEnabledStatus ();

				textView.SetTextColor (Color.Black);
				textView.Text = $"Person to use: {person.Name}";
			}
			else if (position < 0)
			{
				SetVerifyButtonEnabledStatus (false);
				textView.SetTextColor (Color.Red);
				textView.Text = "No person selected for verification warning";
			}
		}


		protected async override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode != REQUEST_SELECT_IMAGE)
			{
				return;
			}

			try
			{
				if (resultCode == Result.Ok)
				{
					var selectedPhoto = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

					if (selectedPhoto != null)
					{
						SetVerifyButtonEnabledStatus (false);
						ClearDetectedFaces ();

						bitmap = selectedPhoto;

						AddLog ($"Image: {data.Data} resized to {bitmap.Width} x {bitmap.Height}");

						await Detect ();
					}
				}
			}
			catch (Exception ex)
			{
				AddLog (ex.Message);
			}
		}


		void ClearDetectedFaces ()
		{
			listView_faces.Visibility = ViewStates.Gone;

			var imageView = FindViewById<ImageView> (Resource.Id.image_0);
			imageView.SetImageResource (Color.Transparent);
		}


		void Select_Image_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
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
				AddLog ($"Request: Verifying face {faceListAdapter.SelectedFace.Id} and person {personListAdapter.SelectedPerson.Id}");
				progressDialog.Show ();
				progressDialog.SetMessage ("Verifying...");
				SetInfo ("Verifying...");

				var person = personListAdapter.SelectedPerson;
				var personGroup = personListAdapter.GetPersonGroup (person);

				var result = await FaceClient.Shared.Verify (faceListAdapter.SelectedFace, person, personGroup);

				if (result != null)
				{
					AddLog ($"Response: Success. Face {faceListAdapter.SelectedFace.Id} {person.Id} {(result.IsIdentical ? "" : "don't")} belong to person {person.Id}");
					SetInfo ($"{(result.IsIdentical ? "The same person" : "Different persons")}. The confidence is {result.Confidence}");
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
			SetAllButtonEnabledStatus (true);
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (VerificationLogActivity));
			StartActivity (intent);
		}


		void SetSelectImageButtonEnabledStatus (bool isEnabled)
		{
			select_image.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void SetVerifyButtonEnabledStatus (bool? isEnabled = null)
		{
			verify.Enabled = isEnabled ?? (faceListAdapter?.SelectedFace != null && personListAdapter.SelectedPerson != null);
		}


		void SetAllButtonEnabledStatus (bool isEnabled)
		{
			select_image.Enabled = isEnabled;
			manage_persons.Enabled = isEnabled;
			verify.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void ListView_Persons_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			SetPersonSelected (e.Position);
		}


		void ListView_Faces_0_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			if (faceListAdapter.SelectedFace != faceListAdapter [e.Position])
			{
				faceListAdapter.SetSelectedIndex (e.Position);

				var imageView = FindViewById<ImageView> (Resource.Id.image_0);
				imageView.SetImageBitmap (faceListAdapter.GetThumbnailForPosition (e.Position));

				SetInfo ("");
				SetVerifyButtonEnabledStatus ();
			}
		}


		async Task Detect ()
		{
			await ExecuteDetection ();

			SetSelectImageButtonEnabledStatus (false);
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

				faceListAdapter = new FaceImageListAdapter (faces, bitmap);

				// Set the default face ID to the ID of first face, if one or more faces are detected.
				if (faces?.Count > 0)
				{
					faceListAdapter.SetSelectedIndex (0);

					// Show the thumbnail of the default face.
					var imageView = FindViewById<ImageView> (Resource.Id.image_0);
					imageView.SetImageBitmap (faceListAdapter.GetThumbnailForPosition (0));

					SetVerifyButtonEnabledStatus ();
				}
				else
				{
					SetInfo ("No face detected!");
				}

				// Show the list of detected face thumbnails.
				listView_faces.Adapter = faceListAdapter;
				listView_faces.Visibility = ViewStates.Visible;

				bitmap = null;
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("No face detected!");
			}

			SetSelectImageButtonEnabledStatus (true);
			progressDialog.Dismiss ();
			SetVerifyButtonEnabledStatus ();
		}


		void SetInfo (String info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		void AddLog (String log)
		{
			LogHelper.AddVerificationLog (log);
		}


		void Manage_Persons_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (PersonGroupListActivity));
			StartActivity (intent);

			SetVerifyButtonEnabledStatus ();
		}


		class PersonListAdapter : BaseAdapter<Person>
		{
			readonly List<PersonGroup> Groups;
			readonly List<Person> People;

			public Person SelectedPerson { get; private set; }

			public PersonListAdapter (List<PersonGroup> groups)
			{
				Groups = groups;
				People = groups.SelectMany (g => g.People).ToList ();
			}


			public override int Count => People?.Count ?? 0;


			public override Person this [int position] => People [position];


			public PersonGroup GetPersonGroup (Person person)
			{
				return Groups.FirstOrDefault (g => g.People.Contains (person));
			}


			public override long GetItemId (int position) => position;


			public void SelectAt (int position)
			{
				SelectedPerson = this [position];

				if (position > 0)
				{
					People.RemoveAt (position);
					People.Insert (0, SelectedPerson);

					NotifyDataSetChanged ();
				}
			}


			public void Select (Person selectedPerson)
			{
				var index = People.IndexOf (selectedPerson);
				SelectAt (index);
			}


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_person_group, parent, false);
				}

				convertView.Id = position;

				var person = People [position];
				var personGroup = Groups.FirstOrDefault (g => g.People.Contains (person));

				convertView.FindViewById<TextView> (Resource.Id.text_person_group).Text = $"{personGroup.Name} - {person.Name}";

				if (position == 0)
				{
					convertView.FindViewById<TextView> (Resource.Id.text_person_group).SetTextColor (Color.ParseColor ("#3399FF"));
				}

				return convertView;
			}
		}
	}
}