using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/find_similar_face_log",
			  ParentActivity = typeof (FindSimilarFaceActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FindSimilarFaceLogActivity : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_find_similar_face_log);

			var logListView = FindViewById<ListView> (Resource.Id.log);
			logListView.Adapter = new LogAdapter (LogType.FindSimilarFaces);
		}
	}
}