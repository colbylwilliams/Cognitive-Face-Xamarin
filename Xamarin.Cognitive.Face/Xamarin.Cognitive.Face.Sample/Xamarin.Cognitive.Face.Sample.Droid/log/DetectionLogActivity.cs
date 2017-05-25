using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/detection_log",
			  ParentActivity = typeof (DetectionActivity),
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetectionLogActivity : AppCompatActivity
	{
		private ListView logListView = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_detection_log);

			logListView = FindViewById<ListView> (Resource.Id.log);
			logListView.Adapter = new LogAdapter (this);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		private class LogAdapter : BaseAdapter
		{
			private List<string> log;
			private DetectionLogActivity activity;

			public LogAdapter (DetectionLogActivity act)
			{
				this.log = LogHelper.GetDetectionLog ();
				this.activity = act;
			}

			public override bool IsEnabled (int position)
			{
				return false;
			}

			public override int Count
			{
				get
				{
					return log.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return log [position];
			}

			public override long GetItemId (int position)
			{
				return position;
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					LayoutInflater layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_log, parent, false);
				}
				convertView.Id = position;

				((TextView) convertView.FindViewById (Resource.Id.log)).Text = log [position];

				return convertView;
			}
		}
	}
}