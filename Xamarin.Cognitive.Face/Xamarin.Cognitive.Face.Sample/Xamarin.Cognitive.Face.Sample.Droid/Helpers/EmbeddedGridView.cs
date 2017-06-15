using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	public class EmbeddedGridView : GridView
	{
		public EmbeddedGridView (Context context, IAttributeSet attributes)
			: base (context, attributes)
		{
		}


		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (widthMeasureSpec, heightMeasureSpec);

			int newHeightMeasureSpec = MeasureSpec.MakeMeasureSpec (MeasuredSizeMask, MeasureSpecMode.AtMost);
		}
	}
}