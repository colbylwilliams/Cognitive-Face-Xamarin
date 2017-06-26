using System;
using Android.App;
using Android.Runtime;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Application]
	public class FaceDemoApp : Application
	{
		public FaceDemoApp (IntPtr javaReference, JniHandleOwnership transfer) : base (javaReference, transfer)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();

			string faceApiKey = "";

			if (string.IsNullOrEmpty (faceApiKey))
			{
				throw new Exception ("No API key set.  Please sign up for a Face API key at https://azure.microsoft.com/en-us/services/cognitive-services/face/");
			}

			FaceClient.Shared.Endpoint = Endpoints.WestUS;
			FaceClient.Shared.SubscriptionKey = faceApiKey;
		}
	}
}