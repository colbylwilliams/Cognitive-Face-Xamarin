using System;
using Foundation;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public interface ISupportGestureAction
	{
		void AttachAction (Action<NSObject> action);

		void DetachAction ();
	}
}