using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/identification_log",
			  ParentActivity = typeof (IdentificationActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class IdentificationLogActivity : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_identification_log);

			var logListView = FindViewById<ListView> (Resource.Id.log);
			logListView.Adapter = new LogAdapter (LogType.Identification);
		}
	}
}