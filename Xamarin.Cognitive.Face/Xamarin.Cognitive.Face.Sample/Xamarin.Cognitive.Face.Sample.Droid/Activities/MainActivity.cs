using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Support.V7.App;
using Android;
using Android.Support.Design.Widget;
using Android.Content.PM;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/app_name",
			  MainLauncher = true,
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : AppCompatActivity
	{
		const int REQUEST_ANDROID_PERMISSIONS = 0;

		Button detection, verification, grouping, findSimilarFace, identification;
		int permissions_granted;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.activity_main);

			detection = FindViewById<Button> (Resource.Id.detection);
			verification = FindViewById<Button> (Resource.Id.verification);
			grouping = FindViewById<Button> (Resource.Id.grouping);
			findSimilarFace = FindViewById<Button> (Resource.Id.findSimilarFace);
			identification = FindViewById<Button> (Resource.Id.identification);

			string camera_permission = Manifest.Permission.Camera;
			string wstorage_permission = Manifest.Permission.WriteExternalStorage;

			if ((CheckSelfPermission (camera_permission) == Permission.Denied) ||
				(CheckSelfPermission (wstorage_permission) == Permission.Denied))
			{
				Snackbar.Make (FindViewById<LinearLayout> (Resource.Id.layout_main), "Internet and write external storage access are required", Snackbar.LengthIndefinite)
						.SetAction ("OK", (b) =>
						 {
							 RequestPermissions (new [] { camera_permission, wstorage_permission }, REQUEST_ANDROID_PERMISSIONS);
						 })
				.Show ();
			}
			else
			{
				permissions_granted = 2;
				CheckIfEnableButtons ();
			}
		}


		public override void OnRequestPermissionsResult (int requestCode, string [] permissions, Permission [] grantResults)
		{
			base.OnRequestPermissionsResult (requestCode, permissions, grantResults);

			switch (requestCode)
			{
				case REQUEST_ANDROID_PERMISSIONS:

					foreach (Permission permission in grantResults)
					{
						if (permission == Permission.Granted)
						{
							permissions_granted++;
						}
					}

					break;
			}

			CheckIfEnableButtons ();
		}


		void CheckIfEnableButtons ()
		{
			detection.Enabled = (permissions_granted == 2);
			verification.Enabled = (permissions_granted == 2);
			grouping.Enabled = (permissions_granted == 2);
			findSimilarFace.Enabled = (permissions_granted == 2);
			identification.Enabled = (permissions_granted == 2);
		}


		protected override void OnResume ()
		{
			base.OnResume ();

			detection.Click += Detection_Click;
			verification.Click += Verification_Click;
			grouping.Click += Grouping_Click;
			findSimilarFace.Click += FindSimilarFace_Click;
			identification.Click += Identification_Click;
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			detection.Click -= Detection_Click;
			verification.Click -= Verification_Click;
			grouping.Click -= Grouping_Click;
			findSimilarFace.Click -= FindSimilarFace_Click;
			identification.Click -= Identification_Click;
		}


		void Detection_Click (object sender, System.EventArgs e)
		{
			var intent = new Intent (this, typeof (DetectionActivity));
			StartActivity (intent);
		}


		void Verification_Click (object sender, System.EventArgs e)
		{
			var intent = new Intent (this, typeof (VerificationMenuActivity));
			StartActivity (intent);
		}


		void Grouping_Click (object sender, System.EventArgs e)
		{
			var intent = new Intent (this, typeof (GroupingActivity));
			StartActivity (intent);
		}


		void FindSimilarFace_Click (object sender, System.EventArgs e)
		{
			var intent = new Intent (this, typeof (FindSimilarFaceActivity));
			StartActivity (intent);
		}


		void Identification_Click (object sender, System.EventArgs e)
		{
			var intent = new Intent (this, typeof (IdentificationActivity));
			StartActivity (intent);
		}
	}
}