using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Java.Util;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample
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
				catch (Face.Droid.Rest.ClientException cex)
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


		internal Task<string> AddFaceForPerson (string personId, string personGroupId, Shared.Face face, Stream photoStream, string userData = null)
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


		internal Task<Shared.Face> GetFaceForPerson (string personId, string personGroupId, string persistedFaceId)
		{
			return Task.Run (() =>
			{
				var persistedFace = Client.GetPersonFace (personGroupId, personId.ToUUID (), persistedFaceId.ToUUID ());
				return persistedFace.ToFace ();
			});
		}


		#endregion


		#region Face


		internal Task<List<Shared.Face>> DetectFacesInPhotoInternal (Stream photoStream, bool returnLandmarks, params FaceAttributeType [] attributes)
		{
			return Task.Run (() =>
			{
				var types = attributes.Select (a => a.ToNativeFaceAttributeType ()).ToArray ();

				var detectedFaces = Client.Detect (photoStream, true, returnLandmarks, types);

				return detectedFaces.Select (f => f.ToFace (returnLandmarks, attributes)).ToList ();
			});
		}


		internal Task<List<SimilarFaceResult>> FindSimilarInternal (string targetFaceId, string [] faceIds, int maxCandidatesReturned = 1)
		{
			return Task.Run (() =>
			{
				var results = Client.FindSimilar (targetFaceId.ToUUID (), faceIds.AsUUIDs (), maxCandidatesReturned);

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



		public Task<Face.Droid.Contract.Face []> Detect (MemoryStream stream, bool returnFaceId, bool returnLandmarks, FaceServiceClientFaceAttributeType [] attributes)
		{
			return Task.Run (() =>
			 {
				 return Client.Detect (stream, true, true, attributes);

			 });
		}

		public Task<Face.Droid.Contract.VerifyResult> Verify (UUID mFaceId0, UUID mFaceId1)
		{
			return Task.Run (() =>
			 {
				 return Client.Verify (mFaceId0, mFaceId1);
			 });
		}

		public Task<Face.Droid.Contract.VerifyResult> Verify (UUID mFaceId, String mPersonGroupId, UUID mPersonId)
		{
			return Task.Run (() =>
			 {
				 return Client.Verify (mFaceId, mPersonGroupId, mPersonId);
			 });
		}


		public Task<Face.Droid.Contract.GroupResult> Group (UUID [] faceIds)
		{
			return Task.Run (() =>
			 {
				 return Client.Group (faceIds);
			 });
		}

		public Task<Face.Droid.Contract.SimilarFace []> FindSimilar (UUID mFaceId, UUID [] mFaceIds, int mMaxNumOfCandidatesReturned)
		{
			return Task.Run (() =>
			 {
				 return Client.FindSimilar (mFaceId, mFaceIds, mMaxNumOfCandidatesReturned);
			 });
		}

		public Task<Face.Droid.Contract.SimilarFace []> FindSimilar (UUID mFaceId, UUID [] mFaceIds, int mMaxNumOfCandidatesReturned, FaceServiceClientFindSimilarMatchMode mMode)
		{
			return Task.Run (() =>
			 {
				 return Client.FindSimilar (mFaceId, mFaceIds, mMaxNumOfCandidatesReturned, mMode);
			 });
		}

		public Task<Face.Droid.Contract.TrainingStatus> GetPersonGroupTrainingStatus (string mPersonGroupId)
		{
			return Task.Run (() =>
			 {
				 return Client.GetPersonGroupTrainingStatus (mPersonGroupId);
			 });
		}

		public Task<Face.Droid.Contract.IdentifyResult []> Identity (string mPersonGroupId, UUID [] mFaceIds, int maxNumOfCandidatesReturned)
		{
			return Task.Run (() =>
			 {
				 return Client.Identity (mPersonGroupId, mFaceIds, maxNumOfCandidatesReturned);
			 });
		}
	}
}