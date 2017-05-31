using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		static FaceClient _shared;
		public static FaceClient Shared => _shared ?? (_shared = new FaceClient ());

		public string SubscriptionKey { get; set; }
		public string Endpoint { get; set; } = Endpoints.WestUS;


		public static class Endpoints
		{
			public const string WestUS = "https://westus.api.cognitive.microsoft.com/face/v1.0/";

			public const string EastUS2 = "https://eastus2.api.cognitive.microsoft.com/face/v1.0/";

			public const string WestCentralUS = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/";

			public const string WestEurope = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/";

			public const string SEAsia = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/";
		}


		public List<PersonGroup> Groups { get; private set; } = new List<PersonGroup> ();


		void RemoveGroup (string personGroupId)
		{
			var theGroup = Groups.FirstOrDefault (g => g.Id == personGroupId);

			if (theGroup != null)
			{
				Groups.Remove (theGroup);
			}
		}


		public Task DeletePersonGroup (PersonGroup personGroup)
		{
			return DeletePersonGroup (personGroup.Id);
		}


		public Task<TrainingStatus> GetGroupTrainingStatus (PersonGroup personGroup)
		{
			return GetGroupTrainingStatus (personGroup.Id);
		}


		public static class ErrorCodes
		{
			public static class TrainingStatus
			{
				public const string PersonGroupNotFound = "PersonGroupNotFound";
				public const string PersonGroupNotTrained = "PersonGroupNotTrained";
			}
		}
	}
}