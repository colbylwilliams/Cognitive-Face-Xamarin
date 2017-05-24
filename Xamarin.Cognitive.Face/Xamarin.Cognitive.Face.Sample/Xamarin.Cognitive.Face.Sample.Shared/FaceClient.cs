using System.Collections.Generic;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		static FaceClient _shared;
		public static FaceClient Shared => _shared ?? (_shared = new FaceClient ());

		public string SubscriptionKey { get; set; }

		public List<PersonGroup> Groups { get; private set; } = new List<PersonGroup> ();


	}
}