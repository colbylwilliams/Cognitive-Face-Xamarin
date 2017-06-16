using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face
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


		public static class ErrorCodes
		{
			public static class TrainingStatus
			{
				public const string PersonGroupNotFound = "PersonGroupNotFound";
				public const string PersonGroupNotTrained = "PersonGroupNotTrained";
			}
		}


		#region Group


		public async Task<List<PersonGroup>> LoadGroupsWithPeople (bool forceRefresh = false)
		{
			try
			{
				//load all groups and people
				var groups = await GetPersonGroups (forceRefresh);

				foreach (var personGroup in groups)
				{
					await GetPeopleForGroup (personGroup, forceRefresh);
				}

				return groups;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<PersonGroup>> GetPersonGroups (bool forceRefresh = false)
		{
			try
			{
				if (Groups.Count == 0 || forceRefresh)
				{
					Groups = await GetGroups ();
				}

				return Groups;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<PersonGroup> GetPersonGroup (string personGroupId)
		{
			try
			{
				return await GetGroup (personGroupId);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			try
			{
				var personGroupId = Guid.NewGuid ().ToString ();

				await CreatePersonGroup (personGroupId, groupName, userData);

				var personGroup = new PersonGroup
				{
					Name = groupName,
					Id = personGroupId,
					UserData = userData,
					People = new List<Person> ()
				};

				Groups.Add (personGroup);

				return personGroup;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			try
			{
				await UpdatePersonGroup (personGroup.Id, groupName, userData);

				personGroup.Name = groupName;
				personGroup.UserData = userData;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task DeletePersonGroup (PersonGroup personGroup)
		{
			try
			{
				await DeletePersonGroup (personGroup.Id);

				if (Groups.Contains (personGroup))
				{
					Groups.Remove (personGroup);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task TrainPersonGroup (PersonGroup personGroup)
		{
			try
			{
				await TrainPersonGroup (personGroup.Id);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		/// <summary>
		/// Gets the group training status: notstarted, running, succeeded, failed
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroup">Person group to get training status for.</param>
		public async Task<TrainingStatus> GetGroupTrainingStatus (PersonGroup personGroup)
		{
			try
			{
				return await GetGroupTrainingStatus (personGroup.Id);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		#endregion


		#region Person


		public async Task<List<Person>> GetPeopleForGroup (PersonGroup personGroup, bool forceRefresh = false)
		{
			try
			{
				if (personGroup.People?.Count > 0 && !forceRefresh)
				{
					return personGroup.People;
				}

				var people = await GetPeopleForGroup (personGroup.Id);

				if (personGroup.PeopleLoaded)
				{
					personGroup.People.Clear ();
					personGroup.People.AddRange (people);
				}
				else
				{
					personGroup.People = people;
				}

				return people;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<Person> CreatePerson (string personName, PersonGroup personGroup, string userData = null)
		{
			try
			{
				var id = await CreatePerson (personName, personGroup.Id, userData);

				if (string.IsNullOrEmpty (id))
				{
					throw new Exception ("CreatePerson Result returned null or empty person Id");
				}

				var person = new Person
				{
					Name = personName,
					Id = id,
					UserData = userData
				};

				if (personGroup.PeopleLoaded)
				{
					personGroup.People.Add (person);
				}

				return person;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task UpdatePerson (Person person, PersonGroup personGroup, string personName, string userData = null)
		{
			try
			{
				await UpdatePerson (person.Id, personGroup.Id, personName, userData);

				person.Name = personName;
				person.UserData = userData;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task DeletePerson (PersonGroup personGroup, Person person)
		{
			try
			{
				await DeletePerson (personGroup.Id, person.Id);

				if (personGroup.PeopleLoaded && personGroup.People.Contains (person))
				{
					personGroup.People.Remove (person);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<Person> GetPerson (PersonGroup personGroup, string personId)
		{
			//if people are already loaded for this group try to find it there...
			if (personGroup.PeopleLoaded)
			{
				var person = personGroup.People.FirstOrDefault (p => p.Id == personId);

				if (person != null)
				{
					return person;
				}
			}

			try
			{
				var person = await GetPerson (personGroup.Id, personId);

				//add them to the group?
				if (personGroup.PeopleLoaded)
				{
					personGroup.People.Add (person);
				}

				return person;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task AddFaceForPerson (Person person, PersonGroup personGroup, Model.Face face, Func<Stream> photoStreamProvider, string userData = null)
		{
			try
			{
				using (var stream = await Task.Run (photoStreamProvider))
				{
					await AddFaceForPerson (person, personGroup, face, stream, userData);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task AddFaceForPerson (Person person, PersonGroup personGroup, Model.Face face, Stream stream, string userData = null)
		{
			try
			{
				var id = await AddFaceForPerson (person.Id, personGroup.Id, face, stream, userData);

				if (string.IsNullOrEmpty (id))
				{
					throw new Exception ("AddFaceForPerson Result returned null or empty face Id");
				}

				face.Id = id;

				face.UpdatePhotoPath ();
				person.Faces.Add (face);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task DeletePersonFace (Person person, PersonGroup personGroup, Model.Face face)
		{
			try
			{
				await DeletePersonFace (person.Id, personGroup.Id, face.Id);

				if (person.Faces.Contains (face))
				{
					person.Faces.Remove (face);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<Model.Face> GetFaceForPerson (Person person, PersonGroup personGroup, string persistedFaceId)
		{
			try
			{
				var persistedFace = await GetFaceForPerson (person.Id, personGroup.Id, persistedFaceId);

				person.Faces.Add (persistedFace);

				return persistedFace;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<Model.Face>> GetFacesForPerson (Person person, PersonGroup personGroup)
		{
			try
			{
				person.Faces.Clear ();

				if (person.FaceIds?.Count > 0)
				{
					foreach (var faceId in person.FaceIds)
					{
						await GetFaceForPerson (person, personGroup, faceId);
					}

					return person.Faces;
				}

				return default (List<Model.Face>);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		#endregion


		#region Face


		public Task<List<Model.Face>> DetectFacesInPhoto (Func<Stream> photoStreamProvider)
		{
			return DetectFacesInPhoto (photoStreamProvider, false);
		}


		public Task<List<Model.Face>> DetectFacesInPhoto (Func<Stream> photoStreamProvider, params FaceAttributeType [] attributes)
		{
			return DetectFacesInPhoto (photoStreamProvider, false, attributes);
		}


		public async Task<List<Model.Face>> DetectFacesInPhoto (Func<Stream> photoStreamProvider, bool returnLandmarks, params FaceAttributeType [] attributes)
		{
			try
			{
				using (var stream = await Task.Run (photoStreamProvider))
				{
					return await DetectFacesInPhoto (stream, returnLandmarks, attributes);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<Model.Face>> DetectFacesInPhoto (Stream photoStream, bool returnLandmarks = false, params FaceAttributeType [] attributes)
		{
			try
			{
				return await DetectFacesInPhotoInternal (photoStream, returnLandmarks, attributes);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<SimilarFaceResult>> FindSimilar (string targetFaceId, string [] faceIdList)
		{
			try
			{
				return await FindSimilarInternal (targetFaceId, faceIdList);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList)
		{
			return FindSimilar (targetFace, faceList, 1, FindSimilarMatchMode.MatchPerson);
		}


		public Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList, int maxCandidatesReturned)
		{
			return FindSimilar (targetFace, faceList, maxCandidatesReturned, FindSimilarMatchMode.MatchPerson);
		}


		public async Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			try
			{
				var faceIdList = faceList.Select (f => f.Id).ToArray ();

				var results = await FindSimilarInternal (targetFace.Id, faceIdList, maxCandidatesReturned, matchMode);

				foreach (var similarFaceResult in results)
				{
					similarFaceResult.Face = faceList.FirstOrDefault (f => f.Id == similarFaceResult.FaceId);
				}

				return results;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<GroupResult> GroupFaces (List<Model.Face> targetFaces)
		{
			try
			{
				var faceIdList = targetFaces.Select (f => f.Id).ToArray ();
				var results = new List<FaceGroup> ();

				var groupResult = await GroupFaces (faceIdList);

				foreach (var faceGroup in groupResult.Groups)
				{
					faceGroup.Faces = targetFaces.Where (f => faceGroup.FaceIds.Contains (f.Id)).ToList ();
				}

				if (groupResult.MessyGroup != null)
				{
					groupResult.MessyGroup.Faces = targetFaces.Where (f => groupResult.MessyGroup.FaceIds.Contains (f.Id)).ToList ();
				}

				return groupResult;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<List<IdentificationResult>> Identify (PersonGroup personGroup, Model.Face detectedFace, int maxNumberOfCandidates = 1)
		{
			return Identify (personGroup, new Model.Face [] { detectedFace }, maxNumberOfCandidates);
		}


		public async Task<List<IdentificationResult>> Identify (PersonGroup personGroup, IEnumerable<Model.Face> detectedFaces, int maxNumberOfCandidates = 1)
		{
			try
			{
				//ensure people are loaded for this group
				await GetPeopleForGroup (personGroup);

				var detectedFaceIds = detectedFaces.Select (f => f.Id).ToArray ();

				var results = await Identify (personGroup.Id, detectedFaceIds, maxNumberOfCandidates);

				System.Diagnostics.Debug.Assert (results.Count <= detectedFaces.Count (), "Number of detected faces passed in expected to be equal or more than the returned result count");

				for (int i = 0; i < results.Count; i++)
				{
					var result = results [i];
					result.Face = detectedFaces.ElementAt (i);

					foreach (var candidate in result.CandidateResults)
					{
						candidate.Person = await GetPerson (personGroup, candidate.PersonId);
					}
				}

				return results;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<VerifyResult> Verify (Model.Face face1, Model.Face face2)
		{
			try
			{
				return await Verify (face1.Id, face2.Id);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<VerifyResult> Verify (Model.Face face, Person person, PersonGroup personGroup)
		{
			try
			{
				return await Verify (face.Id, person.Id, personGroup.Id);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		#endregion
	}
}