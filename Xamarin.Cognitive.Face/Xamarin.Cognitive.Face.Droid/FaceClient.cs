using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Droid;

namespace Xamarin.Cognitive.Face
{
	public partial class FaceClient
	{
		FaceServiceRestClient client;
		FaceServiceRestClient Client => client ?? (client = new FaceServiceRestClient (Endpoint, SubscriptionKey));

		FaceClient () { }


		#region Person Group


		internal Task<List<PersonGroup>> GetGroups ()
		{
			return Task.Run (() =>
			{
				var groups = Client.ListPersonGroups ();

				return groups.Select (g => g.ToPersonGroup ()).ToList ();
			});
		}


		internal Task<PersonGroup> GetGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				var personGroup = Client.GetPersonGroup (personGroupId);

				return personGroup.ToPersonGroup ();
			});
		}


		internal Task CreatePersonGroup (string personGroupId, string groupName, string userData)
		{
			return Task.Run (() =>
			{
				Client.CreatePersonGroup (personGroupId, groupName, userData);
			});
		}


		internal Task UpdatePersonGroup (string personGroupId, string groupName, string userData)
		{
			return Task.Run (() =>
			{
				Client.UpdatePersonGroup (personGroupId, groupName, userData);
			});
		}


		internal Task DeletePersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				Client.DeletePersonGroup (personGroupId);
			});
		}


		internal Task TrainPersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				Client.TrainPersonGroup (personGroupId);
			});
		}


		internal Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					var trainingStatus = Client.GetPersonGroupTrainingStatus (personGroupId);

					return trainingStatus.ToTrainingStatus ();
				}
				catch (Droid.Rest.ClientException cex)
				{
					if (cex.Error?.Code == ErrorCodes.TrainingStatus.PersonGroupNotTrained)
					{
						return TrainingStatus.FromStatus (TrainingStatus.TrainingStatusType.NotStarted);
					}

					throw;
				}
			});
		}


		#endregion


		#region Person


		internal Task<List<Person>> GetPeopleForGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				var arrPeople = Client.ListPersons (personGroupId);

				var people = new List<Person> (
					arrPeople.Select (p => p.ToPerson ())
				);

				return people;
			});
		}


		internal Task<string> CreatePerson (string personName, string personGroupId, string userData)
		{
			return Task.Run (() =>
			{
				var result = Client.CreatePerson (personGroupId, personName, userData);

				return result.PersonId.ToString ();
			});
		}


		internal Task UpdatePerson (string personId, string personGroupId, string personName, string userData)
		{
			return Task.Run (() =>
			{
				Client.UpdatePerson (personGroupId, personId.ToUUID (), personName, userData);
			});
		}


		internal Task DeletePerson (string personGroupId, string personId)
		{
			return Task.Run (() =>
			{
				Client.DeletePerson (personGroupId, personId.ToUUID ());
			});
		}


		internal Task<Person> GetPerson (string personGroupId, string personId)
		{
			return Task.Run (() =>
			{
				var jPerson = Client.GetPerson (personGroupId, personId.ToUUID ());
				return jPerson.ToPerson ();
			});
		}


		internal Task<string> AddFaceForPerson (string personId, string personGroupId, Model.Face face, Stream photoStream, string userData = null)
		{
			return Task.Run (() =>
			{
				var result = Client.AddPersonFace (personGroupId, personId.ToUUID (), photoStream, userData, face.FaceRectangle.ToFaceRect ());

				return result?.PersistedFaceId?.ToString ();
			});
		}


		internal Task DeletePersonFace (string personId, string personGroupId, string faceId)
		{
			return Task.Run (() =>
			{
				Client.DeletePersonFace (personGroupId, personId.ToUUID (), faceId.ToUUID ());
			});
		}


		internal Task<Model.Face> GetFaceForPerson (string personId, string personGroupId, string persistedFaceId)
		{
			return Task.Run (() =>
			{
				var persistedFace = Client.GetPersonFace (personGroupId, personId.ToUUID (), persistedFaceId.ToUUID ());
				return persistedFace.ToFace ();
			});
		}


		#endregion


		#region Face


		internal Task<List<Model.Face>> DetectFacesInPhotoInternal (Stream photoStream, bool returnLandmarks, params FaceAttributeType [] attributes)
		{
			return Task.Run (() =>
			{
				var types = attributes.Select (a => a.AsJavaEnum<FaceServiceClientFaceAttributeType> (false)).ToArray ();

				var detectedFaces = Client.Detect (photoStream, true, returnLandmarks, types);

				return detectedFaces.Select (f => f.ToFace (returnLandmarks, attributes)).ToList ();
			});
		}


		internal Task<List<SimilarFaceResult>> FindSimilarInternal (string targetFaceId, string [] faceIds, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			return Task.Run (() =>
			{
				var results = Client.FindSimilar (targetFaceId.ToUUID (), faceIds.AsUUIDs (), maxCandidatesReturned, matchMode.AsJavaEnum<FaceServiceClientFindSimilarMatchMode> ());

				return results.Select (res => res.ToSimilarFaceResult ()).ToList ();
			});
		}


		internal Task<GroupResult> GroupFaces (string [] targetFaceIds)
		{
			return Task.Run (() =>
			{
				var groupResult = Client.Group (targetFaceIds.AsUUIDs ());

				return groupResult.ToGroupResult ();
			});
		}


		internal Task<List<IdentificationResult>> Identify (string personGroupId, string [] detectedFaceIds, int maxNumberOfCandidates = 1)
		{
			return Task.Run (() =>
			{
				var results = Client.Identity (personGroupId, detectedFaceIds.AsUUIDs (), maxNumberOfCandidates);

				return results.Select (r => r.ToIdentificationResult ()).ToList ();
			});
		}


		internal Task<VerifyResult> Verify (string face1Id, string face2Id)
		{
			return Task.Run (() =>
			{
				var result = Client.Verify (face1Id.ToUUID (), face2Id.ToUUID ());

				return result.ToVerifyResult ();
			});
		}


		internal Task<VerifyResult> Verify (string faceId, string personId, string personGroupId)
		{
			return Task.Run (() =>
			{
				var result = Client.Verify (faceId.ToUUID (), personGroupId, personId.ToUUID ());

				return result.ToVerifyResult ();
			});
		}


		#endregion
	}
}