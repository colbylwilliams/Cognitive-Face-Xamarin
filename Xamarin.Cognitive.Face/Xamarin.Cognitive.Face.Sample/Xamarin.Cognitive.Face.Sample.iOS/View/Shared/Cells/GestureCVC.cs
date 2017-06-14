using System;
using Foundation;
using UIKit;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public abstract class GestureCVC<TGesture> : UICollectionViewCell, ISupportGestureAction
	where TGesture : UIGestureRecognizer, new()
	{
		UIGestureRecognizer.Token GestureToken;

		protected GestureCVC (IntPtr handle) : base (handle)
		{
		}


		protected abstract UIView GestureView { get; }


		public void AttachAction (Action<NSObject> action)
		{
			if (GestureView.GestureRecognizers == null || GestureView.GestureRecognizers?.Length == 0)
			{
				var recognizer = new TGesture ();
				GestureView.AddGestureRecognizer (recognizer);
			}

			if (GestureToken == null)
			{
				GestureToken = GestureView.GestureRecognizers [0]?.AddTarget (action);
			}
		}


		public void DetachAction ()
		{
			if (GestureToken != null)
			{
				if (GestureView.GestureRecognizers != null && GestureView.GestureRecognizers.Length > 0)
				{
					foreach (var recognizer in GestureView.GestureRecognizers)
					{
						if (recognizer is TGesture)
						{
							recognizer.RemoveTarget (GestureToken);
						}
					}
				}

				GestureToken = null;
			}
		}
	}
}