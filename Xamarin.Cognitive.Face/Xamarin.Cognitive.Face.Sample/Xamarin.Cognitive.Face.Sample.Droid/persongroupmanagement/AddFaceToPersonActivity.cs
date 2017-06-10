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
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Shared;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/add_face_to_person",
			  ParentActivity = typeof (PersonActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class AddFaceToPersonActivity : AppCompatActivity
	{
		global::Android.Net.Uri imageUri;
		Bitmap sourceImage;
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

			imageUri = Intent.Data;

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			done_and_save = FindViewById<Button> (Resource.Id.done_and_save);
			gridView = FindViewById<GridView> (Resource.Id.gridView_faces_to_select);
		}


		protected override async void OnResume ()
		{
			base.OnResume ();

			done_and_save.Click += Done_And_Save_Click;

			sourceImage = ContentResolver.LoadSizeLimitedBitmapFromUri (imageUri);

			if (sourceImage != null)
			{
				await ExecuteDetection ();
			}
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			done_and_save.Click -= Done_And_Save_Click;
		}


		async Task ExecuteDetection ()
		{
			progressDialog.Show ();
			AddLog ("Request: Detecting " + imageUri);

			try
			{
				var faces = await FaceClient.Shared.DetectFacesInPhoto (() => sourceImage.AsJpeg ());

				if (faces?.Count > 0)
				{
					SetInfo (faces.Count.ToString () + " face"
							+ (faces.Count != 1 ? "s" : "") + " detected");
				}
				else
				{
					SetInfo ("0 faces detected");
				}

				faceGridViewAdapter = new FaceGridViewAdapter (faces, sourceImage);
				gridView.Adapter = faceGridViewAdapter;
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		async void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (faceGridViewAdapter != null)
			{
				var checkedFaces = faceGridViewAdapter.GetCheckedItems ();

				if (checkedFaces.Length > 0)
				{
					await ExecuteFaceTask (checkedFaces);
				}
				else
				{
					Finish ();
				}
			}
		}


		async Task ExecuteFaceTask (Shared.Face [] faces)
		{
			progressDialog.Show ();

			try
			{
				progressDialog.SetMessage ("Adding face...");
				SetInfo ("Adding face...");

				using (var stream = sourceImage.AsJpeg ())
				{
					foreach (var face in faces)
					{
						AddLog ($"Request: Adding face to person {Person.Id}");

						await FaceClient.Shared.AddFaceForPerson (Person, Group, face, stream);

						var thumbnail = faceGridViewAdapter.GetThumbnail (face);
						face.SavePhotoFromCropped (thumbnail);

						//legacy
						var uri = global::Android.Net.Uri.Parse (face.PhotoPath);
						StorageHelper.SetFaceUri (face.Id, uri.ToString (), Person.Id, this);
					}
				}

				AddLog ("Response: Success. Face(s) " + string.Join (", ", faces.Select (f => f.Id)) + "added to person " + Person.Id);

				Finish ();
			}
			catch (Exception e)
			{
				Log.Error (e);
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		void AddLog (string log)
		{
			LogHelper.AddIdentificationLog (log);
		}


		void SetInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		class FaceGridViewAdapter : BaseAdapter<Shared.Face>, CompoundButton.IOnCheckedChangeListener
		{
			readonly List<Shared.Face> detectedFaces;
			readonly List<Bitmap> faceThumbnails;
			List<bool> faceChecked;

			public FaceGridViewAdapter (List<Shared.Face> detectedFaces, Bitmap photo)
			{
				this.detectedFaces = detectedFaces;

				faceThumbnails = detectedFaces.GenerateThumbnails (photo);

				ResetCheckedItems ();
			}


			public override Shared.Face this [int position] => detectedFaces [position];


			public override int Count => detectedFaces.Count;


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_checkbox, parent, false);
				}

				convertView.Id = position;

				var imageView = convertView.FindViewById<ImageView> (Resource.Id.image_face);
				imageView.SetImageBitmap (faceThumbnails [position]);

				var checkBox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox_face);
				checkBox.Tag = position;
				checkBox.Checked = faceChecked [position];
				checkBox.SetOnCheckedChangeListener (this);

				return convertView;
			}


			public Bitmap GetThumbnail (Shared.Face face)
			{
				var position = detectedFaces.IndexOf (face);

				if (position > -1)
				{
					return faceThumbnails [position];
				}

				return null;
			}


			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				var position = (int) buttonView.Tag;
				faceChecked [position] = isChecked;
			}


			public Shared.Face [] GetCheckedItems ()
			{
				return detectedFaces.Where ((f, index) => faceChecked [index]).ToArray ();
			}


			public void ResetCheckedItems ()
			{
				faceChecked = new List<bool> (detectedFaces.Select (g => false));
			}
		}
	}
}