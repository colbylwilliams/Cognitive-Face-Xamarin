using System;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupPersonCVC : GestureCVC<UILongPressGestureRecognizer>
	{
		public GroupPersonCVC (IntPtr handle) : base (handle)
		{
		}


		protected override UIView GestureView => ImageView;


		public void SetPerson (Person person, int cellActionTag, int faceIndex)
		{
			ImageView.Tag = cellActionTag; //keep track of the person this imageview is for - used in longPressAction

			if (person.Faces?.Count > 0)
			{
				var face = person.Faces? [faceIndex];

				if (face != null)
				{
					TextView.Text = $"Face #{faceIndex + 1}";
					ImageView.Image = face.GetImage ();
					ImageView.RemoveBorder ();
				}
			}
			else
			{
				ImageView.Image = null;
				ImageView.AddBorder (UIColor.Red, 2);
			}
		}
	}
}