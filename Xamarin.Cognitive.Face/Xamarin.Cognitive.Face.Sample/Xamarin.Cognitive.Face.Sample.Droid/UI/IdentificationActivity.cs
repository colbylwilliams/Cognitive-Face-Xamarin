using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/identification",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class IdentificationActivity : AppCompatActivity
	{
		Button manageButton;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_identification);

			manageButton = FindViewById<Button> (Resource.Id.manage_person_groups);
		}


		protected override void OnResume ()
		{
			base.OnResume ();

			manageButton.Click += managePersonGroups;
		}


		protected override void OnPause ()
		{
			manageButton.Click -= managePersonGroups;

			base.OnPause ();
		}


		void managePersonGroups (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (PersonGroupListActivity));
			StartActivity (intent);
		}
	}
}