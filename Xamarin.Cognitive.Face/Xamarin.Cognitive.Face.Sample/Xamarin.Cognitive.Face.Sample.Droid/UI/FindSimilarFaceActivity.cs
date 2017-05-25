using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/find_similar_faces",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class FindSimilarFaceActivity : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_find_similar_face);
		}
	}
}