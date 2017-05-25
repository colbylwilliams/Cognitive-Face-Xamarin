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
		private Button detection, verification, grouping, findSimilarFace, identification = null;
		private const int REQUEST_ANDROID_PERMISSIONS = 0;
		private int permissions_granted = 0;

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
				case (int) REQUEST_ANDROID_PERMISSIONS:

					foreach (Permission permission in grantResults)
					{
						if (permission == Permission.Granted)
						{
							permissions_granted++;
						}
					}

					break;
				default:
					break;
			}
			CheckIfEnableButtons ();
		}

		private void CheckIfEnableButtons ()
		{
			detection.Enabled = (permissions_granted == 2) ? true : false;
			verification.Enabled = (permissions_granted == 2) ? true : false;
			grouping.Enabled = (permissions_granted == 2) ? true : false;
			findSimilarFace.Enabled = (permissions_granted == 2) ? true : false;
			identification.Enabled = (permissions_granted == 2) ? true : false;
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

		private void Detection_Click (object sender, System.EventArgs e)
		{
			Intent intent = new Intent (this, typeof (DetectionActivity));
			this.StartActivity (intent);
		}

		private void Verification_Click (object sender, System.EventArgs e)
		{
			Intent intent = new Intent (this, typeof (VerificationMenuActivity));
			this.StartActivity (intent);
		}

		private void Grouping_Click (object sender, System.EventArgs e)
		{
			Intent intent = new Intent (this, typeof (GroupingActivity));
			this.StartActivity (intent);
		}

		private void FindSimilarFace_Click (object sender, System.EventArgs e)
		{
			Intent intent = new Intent (this, typeof (FindSimilarFaceActivity));
			this.StartActivity (intent);
		}

		private void Identification_Click (object sender, System.EventArgs e)
		{
			Intent intent = new Intent (this, typeof (IdentificationActivity));
			this.StartActivity (intent);
		}
	}
}