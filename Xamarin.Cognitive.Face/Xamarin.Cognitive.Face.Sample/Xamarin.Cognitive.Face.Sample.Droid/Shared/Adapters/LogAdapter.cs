using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters
{
	public class LogAdapter : BaseAdapter
	{
		readonly List<string> log;

		public LogAdapter (LogType logType)
		{
			log = LogHelper.GetLog (logType);
		}


		public override bool IsEnabled (int position) => false;


		public override int Count => log?.Count ?? 0;


		public override Java.Lang.Object GetItem (int position) => log [position];


		public override long GetItemId (int position) => position;


		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			if (convertView == null)
			{
				var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
				convertView = layoutInflater.Inflate (Resource.Layout.item_log, parent, false);
			}

			convertView.Id = position;

			convertView.FindViewById<TextView> (Resource.Id.log).Text = log [position];

			return convertView;
		}
	}
}