using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/grouping_log",
			  ParentActivity = typeof (GroupingActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class GroupingLogActivity : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_grouping_log);

			var listView = FindViewById<ListView> (Resource.Id.log);
			listView.Adapter = new LogAdapter (LogType.Grouping);
		}
	}
}