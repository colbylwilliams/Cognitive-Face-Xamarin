using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face
{
	/// <summary>
	/// Face client is used to communicate with the Face API.
	/// </summary>
	public partial class FaceClient
	{
		static FaceClient _shared;

		/// <summary>
		/// Gets the static/shared instance of this Singleton FaceClient client.
		/// </summary>
		/// <value>The static/shared instance of this Singleton FaceClient client.</value>
		public static FaceClient Shared => _shared ?? (_shared = new FaceClient ());


		/// <summary>
		/// Gets or sets the subscription key to be used for this FaceClient client.
		/// </summary>
		/// <value>The subscription key.</value>
		/// <remarks>Endpoints and subscription keys are linked - you must have an API key for the endpoint being used.</remarks>
		public string SubscriptionKey { get; set; }


		/// <summary>
		/// Gets or sets the Azure Face API endpoint to use for this FaceClient client.
		/// </summary>
		/// <value>The endpoint.</value>
		/// <remarks>Defaults to WestUS.  Endpoints and subscription keys are linked - you must have an API key for the endpoint being used.</remarks>
		public string Endpoint { get; set; } = Endpoints.WestUS;


		/// <summary>
		/// Gets a cached list of the <see cref="PersonGroup"/> groups that have already been loaded this session via the <see cref="GetPersonGroups"/> or similar methods.
		/// </summary>
		/// <value>A list of <see cref="PersonGroup"/> objects.</value>
		public List<PersonGroup> Groups { get; private set; } = new List<PersonGroup> ();


		#region Group


		/// <summary>
		/// Retrieves the <see cref="PersonGroup"/> groups, and any <see cref="Person"/> people that are a part of the group.
		/// </summary>
		/// <returns>The list of groups with people.</returns>
		/// <param name="forceRefresh">If set to <c>true</c>, force refresh of the groups list.  This forces all groups to reload from the server and clears the currently cached list of groups.</param>
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


		/// <summary>
		/// Retrieves the list of all <see cref="PersonGroup"/>.
		/// </summary>
		/// <returns>The list of groups.</returns>
		/// <param name="forceRefresh">If set to <c>true</c>, force refresh of the groups list.  This forces all groups to reload from the server and clears the currently cached list of groups.</param>
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


		/// <summary>
		/// Retrieves the <see cref="PersonGroup"/> with the given Id.
		/// </summary>
		/// <returns>The person group.</returns>
		/// <param name="personGroupId">Person group identifier.</param>
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


		/// <summary>
		/// Creates the <see cref="PersonGroup"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <returns>The person group.</returns>
		/// <param name="groupName">The name of the new group.</param>
		/// <param name="userData">A custom user data string to store with the group.</param>
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


		/// <summary>
		/// Updates the <see cref="PersonGroup"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <param name="personGroup">The <see cref="PersonGroup"/> to update.</param>
		/// <param name="groupName">The updated name of the group.</param>
		/// <param name="userData">An updated custom user data string to store with the group.</param>
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


		/// <summary>
		/// Deletes the <see cref="PersonGroup"/> group.
		/// </summary>
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


		/// <summary>
		/// Queues a <see cref="PersonGroup"/> training task.  The training task may not be started immediately.
		/// </summary>
		/// <param name="personGroup">The <see cref="PersonGroup"/> to begin a training task for.</param>
		/// <seealso cref="GetGroupTrainingStatus(PersonGroup)"/>
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
		/// Gets the <see cref="TrainingStatus"/> for the given <see cref="PersonGroup"/>: notstarted, running, succeeded, failed.
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroup"><see cref="PersonGroup"/> to get training status for.</param>
		/// <seealso cref="TrainPersonGroup(PersonGroup)"/>
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


		/// <summary>
		/// Gets the group's list of <see cref="Person"/>.
		/// </summary>
		/// <returns>A list of <see cref="Person"/> for the group.</returns>
		/// <param name="personGroup"><see cref="PersonGroup"/> to get people for.</param>
		/// <param name="forceRefresh">If set to <c>true</c>, force refresh of the cached list of people for this group.  This forces all people for this group to reload from the server and clears the currently cached list of people for this group.</param>
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


		/// <summary>
		/// Creates a <see cref="Person"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <returns>The person.</returns>
		/// <param name="personName">The name of the new person.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> this person will be a part of.</param>
		/// <param name="userData">A custom user data string to store with the person.</param>
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


		/// <summary>
		/// Updates a <see cref="Person"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> to update.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> this person is a part of.</param>
		/// <param name="personName">The name of the updated person.</param>
		/// <param name="userData">A custom user data string to store with the person.</param>
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


		/// <summary>
		/// Deletes the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="person">The <see cref="Person"/> to delete.</param>
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


		/// <summary>
		/// Gets the <see cref="Person"/> with the specified Id and belonging to the given <see cref="PersonGroup"/>.
		/// </summary>
		/// <returns>The <see cref="Person"/>.</returns>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to get.</param>
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


		/// <summary>
		/// Adds (saves/persists) a detected <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> to add a face for.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="face">The detected <see cref="Face"/> to add.  This will typically come from the Detect method.</param>
		/// <param name="photoStreamProvider">A <see cref="Func{Stream}"/> that provides a stream to the image data containing the Face.</param>
		/// <param name="userData">A custom user data string to store with the person's Face.</param>
		/// <remarks>Note that photoStreamProvider should return a stream that can be disposed - this will be used in a using() statement on a background thread.  
		/// The image stream provided should be the same original image that was used to detect the Face - the <see cref="FaceRectangle"/> must be valid and should contain the correct face.</remarks>
		/// <seealso cref="DetectFacesInPhoto(Func{Stream})"/>
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


		/// <summary>
		/// Adds (saves/persists) a detected <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> to add a face for.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="face">The detected <see cref="Face"/> to add.  This will typically come from the Detect method.</param>
		/// <param name="stream">A stream to the image data containing the Face.</param>
		/// <param name="userData">A custom user data string to store with the person's Face.</param>
		/// <remarks>The Stream passed in to this method will NOT be disposed and should be handled by the calling client code.  
		/// The image stream provided should be the same original image that was used to detect the Face - the <see cref="FaceRectangle"/> must be valid and should contain the correct face.</remarks>
		/// <seealso cref="DetectFacesInPhoto(Stream, bool, FaceAttributeType [])"/>
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
				person.Faces.Add (face);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		/// <summary>
		/// Deletes a persisted <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="face">The persisted <see cref="Face"/> to delete.</param>
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


		/// <summary>
		/// Gets a persisted <see cref="Face"/> with the given <c>persistedFaceId</c> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="persistedFaceId">The Id of the persisted <see cref="Face"/> to retrieve.</param>
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


		/// <summary>
		/// Gets a list of all persisted <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="person">The <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the person is a part of.</param>
		/// <remarks>Note that this will clear any existing faces currently in the Faces collection of the <see cref="Person"/>.  
		/// Every persisted face Id found in the Person's FaceIds collection will be retrieved.</remarks>
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


		/// <summary>
		/// Loads all persisted <see cref="Face"/> for each <see cref="Person"/> in the given <see cref="PersonGroup"/>.
		/// </summary>
		/// <param name="personGroup">The <see cref="PersonGroup"/> to load faces for.</param>
		/// <remarks>Note that this will clear any existing faces currently in the Faces collection of any <see cref="Person"/>.  
		/// Every persisted face Id found in each Person's FaceIds collection will be retrieved.  
		/// This method assumes group people have already been loaded.</remarks>
		/// <seealso cref="GetPeopleForGroup(PersonGroup, bool)"/>
		public async Task LoadFacesForAllGroupPeople (PersonGroup personGroup)
		{
			foreach (var person in personGroup.People)
			{
				if (person.Faces?.Count == 0)
				{
					await GetFacesForPerson (person, personGroup);
				}
			}
		}


		#endregion


		#region Face


		/// <summary>
		/// Returns a list of <see cref="Face"/> that have been detected in the given image stream (loaded via <c>photoStreamProvider</c>).
		/// </summary>
		/// <param name="photoStreamProvider">A <see cref="Func{Stream}"/> that provides a stream to the image data containing the image to run facial detection on.</param>
		/// <remarks>Note that photoStreamProvider should return a stream that can be disposed - this will be used in a using() statement on a background thread.</remarks>
		public Task<List<Model.Face>> DetectFacesInPhoto (Func<Stream> photoStreamProvider)
		{
			return DetectFacesInPhoto (photoStreamProvider, false);
		}


		/// <summary>
		/// Returns a list of <see cref="Face"/> that have been detected in the given image stream (loaded via <c>photoStreamProvider</c>).
		/// </summary>
		/// <param name="photoStreamProvider">A <see cref="Func{Stream}"/> that provides a stream to the image data containing the image to run facial detection on.</param>
		/// <param name="attributes">A list of any <see cref="FaceAttributeType"/> to detect and return, if any.</param>
		/// <remarks>Note that photoStreamProvider should return a stream that can be disposed - this will be used in a using() statement on a background thread.</remarks>
		public Task<List<Model.Face>> DetectFacesInPhoto (Func<Stream> photoStreamProvider, params FaceAttributeType [] attributes)
		{
			return DetectFacesInPhoto (photoStreamProvider, false, attributes);
		}


		/// <summary>
		/// Returns a list of <see cref="Face"/> that have been detected in the given image stream (loaded via <c>photoStreamProvider</c>).
		/// </summary>
		/// <param name="photoStreamProvider">A <see cref="Func{Stream}"/> that provides a stream to the image data containing the image to run facial detection on.</param>
		/// <param name="returnLandmarks"><c>true</c> to return facial landmarks, otherwise <c>false</c></param>
		/// <param name="attributes">A list of any <see cref="FaceAttributeType"/> to detect and return, if any.</param>
		/// <remarks>Note that photoStreamProvider should return a stream that can be disposed - this will be used in a using() statement on a background thread.</remarks>
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


		/// <summary>
		/// Returns a list of <see cref="Face"/> that have been detected in the given image stream.
		/// </summary>
		/// <param name="photoStream">A <see cref="Stream"/> to the image data containing the image to run facial detection on.</param>
		/// <param name="returnLandmarks"><c>true</c> to return facial landmarks, otherwise <c>false</c></param>
		/// <param name="attributes">A list of any <see cref="FaceAttributeType"/> to detect and return, if any.</param>
		/// <remarks>The Stream passed in to this method will NOT be disposed and should be handled by the calling client code.</remarks>
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


		/// <summary>
		/// Finds faces similar to <c>targetFace</c> in the given <c>faceList</c>.
		/// </summary>
		/// <returns>A list of <see cref="SimilarFaceResult"/> indicating similar face(s) and associated confidence factor.</returns>
		/// <param name="targetFace">The target <see cref="Face"/> to find similar faces to.</param>
		/// <param name="faceList">The face list containing faces to compare to <c>targetFace</c>.</param>
		public Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList)
		{
			return FindSimilar (targetFace, faceList, 1, FindSimilarMatchMode.MatchPerson);
		}


		/// <summary>
		/// Finds faces similar to <c>targetFace</c> in the given <c>faceList</c>.
		/// </summary>
		/// <returns>A list of <see cref="SimilarFaceResult"/> indicating similar face(s) and associated confidence factor.</returns>
		/// <param name="targetFace">The target <see cref="Face"/> to find similar faces to.</param>
		/// <param name="faceList">The face list containing faces to compare to <c>targetFace</c>.</param>
		/// <param name="maxCandidatesReturned">The maximum number of candidate faces to return.</param>
		/// <remarks><c>maxCandidatesReturned</c> is not currently respsected on iOS due to native SDK limiations.</remarks>
		public Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList, int maxCandidatesReturned)
		{
			return FindSimilar (targetFace, faceList, maxCandidatesReturned, FindSimilarMatchMode.MatchPerson);
		}


		/// <summary>
		/// Finds faces similar to <c>targetFace</c> in the given <c>faceList</c>.
		/// </summary>
		/// <returns>A list of <see cref="SimilarFaceResult"/> indicating similar face(s) and associated confidence factor.</returns>
		/// <param name="targetFace">The target <see cref="Face"/> to find similar faces to.</param>
		/// <param name="faceList">The face list containing faces to compare to <c>targetFace</c>.</param>
		/// <param name="maxCandidatesReturned">The maximum number of candidate faces to return.</param>
		/// <param name="matchMode">The <see cref="FindSimilarMatchMode"/> to use when comparing - matchFace or matchPerson (default).</param>
		/// <remarks><c>maxCandidatesReturned</c> and <c>matchMode</c> are not currently respsected on iOS due to native SDK limiations.</remarks>
		public async Task<List<SimilarFaceResult>> FindSimilar (Model.Face targetFace, List<Model.Face> faceList, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			try
			{
				var faceIdList = faceList.Select (f => f.Id).ToArray ();

				var results = await FindSimilar (targetFace.Id, faceIdList, maxCandidatesReturned, matchMode);

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


		/// <summary>
		/// Groups similar faces within the <c>targetFaces</c> list and returns a <see cref="GroupResult"/> with results of the grouping operation.
		/// </summary>
		/// <returns>A <see cref="GroupResult"/> containing <see cref="FaceGroup"/> groups with similar faces and any leftover/messy group.</returns>
		/// <param name="targetFaces">The list of target <see cref="Face"/> to perform a grouping operation on.</param>
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


		/// <summary>
		/// Attempts to Identify the given <see cref="Face"/> detectedFace against a trained <see cref="PersonGroup"/> containing 1 or more faces.
		/// </summary>
		/// <returns>A list of <see cref="IdentificationResult"/> containing <see cref="CandidateResult"/> indicating potential identification matches and the confidence factor for the match.</returns>
		/// <param name="personGroup">The <see cref="PersonGroup"/> to identify a <see cref="Face"/> against.</param>
		/// <param name="detectedFace">A detected face to use for the identification.</param>
		/// <param name="maxNumberOfCandidates">The max number of candidate matches to return.</param>
		public Task<List<IdentificationResult>> Identify (PersonGroup personGroup, Model.Face detectedFace, int maxNumberOfCandidates = 1)
		{
			return Identify (personGroup, new Model.Face [] { detectedFace }, maxNumberOfCandidates);
		}


		/// <summary>
		/// Attempts to Identify the given <see cref="Face"/> list <c>detectedFaces</c> against a trained <see cref="PersonGroup"/> containing 1 or more faces.
		/// </summary>
		/// <returns>A list of <see cref="IdentificationResult"/> containing <see cref="CandidateResult"/> indicating potential identification matches and the confidence factor for the match.</returns>
		/// <param name="personGroup">The <see cref="PersonGroup"/> to identify a <see cref="Face"/> against.</param>
		/// <param name="detectedFaces">A list of detected faces to use for the identification.</param>
		/// <param name="maxNumberOfCandidates">The max number of candidate matches to return.</param>
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


		/// <summary>
		/// Verifies that the specified faces belong to the same person.
		/// </summary>
		/// <returns>A <see cref="VerifyResult"/> indicating equivalence, with a confidence factor.</returns>
		/// <param name="face1">The first <see cref="Face"/>.</param>
		/// <param name="face2">The second <see cref="Face"/>.</param>
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


		/// <summary>
		/// Verifies that the given face belongs to the specified <see cref="Person"/>.
		/// </summary>
		/// <returns>A <see cref="VerifyResult"/> indicating equivalence, with a confidence factor.</returns>
		/// <param name="face">The <see cref="Face"/> to verify.</param>
		/// <param name="person">The <see cref="Person"/> to verify the <c>face</c> against.</param>
		/// <param name="personGroup">The <see cref="PersonGroup"/> the given person belongs to.</param>
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