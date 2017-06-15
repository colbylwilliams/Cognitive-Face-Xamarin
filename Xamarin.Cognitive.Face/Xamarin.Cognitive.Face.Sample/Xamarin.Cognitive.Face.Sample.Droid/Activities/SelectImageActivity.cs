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
		const int REQUEST_TAKE_PHOTO = 0;
		const int REQUEST_SELECT_IMAGE_IN_ALBUM = 1;

		global::Android.Net.Uri uriPhotoTaken;
		Button button_take_a_photo, button_select_a_photo_in_album;
		TextView info;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

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


		void Button_Take_A_Photo_Click (object sender, EventArgs e)
		{
			var intent = new Intent (MediaStore.ActionImageCapture);

			if (intent.ResolveActivity (PackageManager) != null)
			{
				var storageDir = GetExternalFilesDir (global::Android.OS.Environment.DirectoryPictures);

				try
				{
					var file = File.CreateTempFile ("IMG_", ".jpg", storageDir);
					uriPhotoTaken = global::Android.Net.Uri.FromFile (file);
					intent.PutExtra (MediaStore.ExtraOutput, uriPhotoTaken);

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
			//Intent intent;

			//if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
			//{
			//intent = new Intent (Intent.ActionOpenDocument);
			//intent.AddFlags (ActivityFlags.GrantPersistableUriPermission);
			//}
			//else
			//{
			//	intent = new Intent (Intent.ActionGetContent);
			//}

			var intent = new Intent (Intent.ActionGetContent);
			intent.SetType ("image/*");
			//intent.AddFlags (ActivityFlags.GrantReadUriPermission);

			if (intent.ResolveActivity (PackageManager) != null)
			{
				StartActivityForResult (intent, REQUEST_SELECT_IMAGE_IN_ALBUM);
			}
		}


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok)
			{
				switch (requestCode)
				{
					case REQUEST_TAKE_PHOTO:
						OnActivityResultCase (data);
						break;
					case REQUEST_SELECT_IMAGE_IN_ALBUM:
						OnActivityResultCase (data);
						break;
				}
			}
		}


		void OnActivityResultCase (Intent data)
		{
			global::Android.Net.Uri imageUri;

			if (data == null || data.Data == null)
			{
				imageUri = uriPhotoTaken;
			}
			else
			{
				imageUri = data.Data;
			}

			var intent = new Intent ();
			intent.SetData (imageUri);
			SetResult (Result.Ok, intent);

			Finish ();
		}


		void SetInfo (string msg)
		{
			info.Text = msg;
		}
	}
}