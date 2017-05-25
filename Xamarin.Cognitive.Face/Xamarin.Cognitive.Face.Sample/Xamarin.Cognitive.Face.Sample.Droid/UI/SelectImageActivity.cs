using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/select_an_image",
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class SelectImageActivity : AppCompatActivity
	{
		private const int REQUEST_TAKE_PHOTO = 0;
		private const int REQUEST_SELECT_IMAGE_IN_ALBUM = 1;
		private global::Android.Net.Uri mUriPhotoTaken;
		private Button button_take_a_photo, button_select_a_photo_in_album = null;
		private TextView info = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_select_image);

			button_take_a_photo = FindViewById<Button> (Resource.Id.button_take_a_photo);
			button_take_a_photo.Click += Button_Take_A_Photo_Click;

			button_select_a_photo_in_album = FindViewById<Button> (Resource.Id.button_select_a_photo_in_album);
			button_select_a_photo_in_album.Click += Button_Select_A_Photo_In_Album_Click;

			info = FindViewById<TextView> (Resource.Id.info);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			button_take_a_photo.Click -= Button_Take_A_Photo_Click;
			button_select_a_photo_in_album.Click -= Button_Select_A_Photo_In_Album_Click;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutParcelable ("ImageUri", mUriPhotoTaken);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
			mUriPhotoTaken = (global::Android.Net.Uri) savedInstanceState.GetParcelable ("ImageUri");
		}

		void Button_Take_A_Photo_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);

			if (intent.ResolveActivity (PackageManager) != null)
			{
				File storageDir = GetExternalFilesDir (global::Android.OS.Environment.DirectoryPictures);

				try
				{
					File file = File.CreateTempFile ("IMG_", ".jpg", storageDir);
					mUriPhotoTaken = global::Android.Net.Uri.FromFile (file);
					intent.PutExtra (MediaStore.ExtraOutput, mUriPhotoTaken);
					StartActivityForResult (intent, REQUEST_TAKE_PHOTO);
				}
				catch (IOException ex)
				{
					SetInfo (ex.Message);
				}
			}
		}

		void Button_Select_A_Photo_In_Album_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (Intent.ActionGetContent);
			intent.SetType ("image/*");

			if (intent.ResolveActivity (PackageManager) != null)
			{
				StartActivityForResult (intent, REQUEST_SELECT_IMAGE_IN_ALBUM);
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode)
			{
				case REQUEST_TAKE_PHOTO:
					OnActivityResultCase (resultCode, data);
					break;
				case REQUEST_SELECT_IMAGE_IN_ALBUM:
					OnActivityResultCase (resultCode, data);
					break;
				default:
					break;
			}
		}

		private void OnActivityResultCase (Result resultCode, Intent data)
		{
			if (resultCode == Result.Ok)
			{
				global::Android.Net.Uri imageUri;

				if (data == null || data.Data == null)
				{
					imageUri = mUriPhotoTaken;
				}
				else
				{
					imageUri = data.Data;
				}

				Intent intent = new Intent ();
				intent.SetData (imageUri);
				SetResult (Result.Ok, intent);
				this.Finish ();
			}
		}

		private void SetInfo (string _info)
		{
			this.info.Text = _info;
		}
	}
}