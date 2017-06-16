using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/verification",
			  ParentActivity = typeof (MainActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class VerificationMenuActivity : AppCompatActivity
	{
		Button select_face_face_verification, select_face_person_verification;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_verification_menu);

			select_face_face_verification = FindViewById<Button> (Resource.Id.select_face_face_verification);
			select_face_face_verification.Click += Select_Face_Face_Verification_Click;

			select_face_person_verification = FindViewById<Button> (Resource.Id.select_face_person_verification);
			select_face_person_verification.Click += Select_Face_Person_Verification_Click;
		}


		protected override void OnDestroy ()
		{
			base.OnDestroy ();

			select_face_face_verification.Click -= Select_Face_Face_Verification_Click;
			select_face_person_verification.Click -= Select_Face_Person_Verification_Click;
		}


		void Select_Face_Face_Verification_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (FaceVerificationActivity));
			StartActivity (intent);
		}


		void Select_Face_Person_Verification_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (PersonVerificationActivity));
			StartActivity (intent);
		}
	}
}