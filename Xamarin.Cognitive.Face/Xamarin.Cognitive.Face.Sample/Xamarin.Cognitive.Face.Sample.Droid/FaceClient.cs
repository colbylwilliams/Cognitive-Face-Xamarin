using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Java.Util;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		FaceServiceRestClient client;
		FaceServiceRestClient Client => client ?? (client = new FaceServiceRestClient (SubscriptionKey));

		FaceClient () { }


		#region Groups


		public Task<List<PersonGroup>> GetGroups (bool forceRefresh = false)
		{
			try
			{
				if (Groups.Count == 0 || forceRefresh)
				{
					var groups = Client.ListPersonGroups ();

					Groups = new List<PersonGroup> (
						groups.Select (g => new PersonGroup
						{
							Id = g.PersonGroupId,
							Name = g.Name,
							UserData = g.UserData
						})
					);
				}

				return Task.FromResult (Groups);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		public Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			try
			{
				var personGroupId = Guid.NewGuid ().ToString ();

				Client.CreatePersonGroup (personGroupId, groupName, userData);

				var group = new PersonGroup
				{
					Name = groupName,
					Id = personGroupId,
					UserData = userData
				};

				Groups.Add (group);

				return Task.FromResult (group);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		public Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			try
			{
				Client.UpdatePersonGroup (personGroup.Id, groupName, userData);

				personGroup.Name = groupName;
				personGroup.UserData = userData;

				return Task.FromResult (true);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		public Task TrainGroup (PersonGroup personGroup)
		{
			try
			{
				Client.TrainPersonGroup (personGroup.Id);

				return Task.FromResult (true);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		#endregion


		#region Person


		public Task<Person> CreatePerson (string personName, PersonGroup group, string userData = null)
		{
			try
			{
				var result = Client.CreatePerson (group.Id, personName, userData);

				var id = result.PersonId.ToString ();

				if (string.IsNullOrEmpty (id))
				{
					throw new Exception ("CreatePersonResult returned invalid person Id");
				}

				var person = new Person
				{
					Name = personName,
					Id = id,
					UserData = userData
				};

				group.People.Add (person);

				return Task.FromResult (person);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		public Task UpdatePerson (Person person, PersonGroup group, string personName, string userData = null)
		{
			try
			{
				Client.UpdatePerson (group.Id, UUID.FromString (person.Id), personName, userData);

				person.Name = personName;
				person.UserData = userData;

				return Task.FromResult (true);
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
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

		public Task DeletePersonGroup (string mPersonGroupId)
		{
			return Task.Run (() =>
			 {
				 Client.DeletePersonGroup (mPersonGroupId);
			 });
		}

		public Task CreatePersonGroup (string mPersonGroupId, string name, string userData)
		{
			return Task.Run (() =>
			 {
				 Client.CreatePersonGroup (mPersonGroupId, name, userData);
			 });
		}

		public Task DeletePerson (string mPersonGroupId, UUID mPersonId)
		{
			return Task.Run (() =>
			 {
				 Client.DeletePerson (mPersonGroupId, mPersonId);
			 });
		}

		public Task TrainPersonGroup (string mPersonGroupId)
		{
			return Task.Run (() =>
			 {
				 Client.TrainPersonGroup (mPersonGroupId);
			 });
		}

		public Task DeletePersonFace (string mPersonGroupId, UUID mPersonId, UUID mFaceId)
		{
			return Task.Run (() =>
			 {
				 Client.DeletePersonFace (mPersonGroupId, mPersonId, mFaceId);
			 });
		}

		public Task<Face.Droid.Contract.CreatePersonResult> CreatePerson (string mPersonGroupId, string name, string userData)
		{
			return Task.Run (() =>
			 {
				 return Client.CreatePerson (mPersonGroupId, name, userData);
			 });
		}

		public Task<Face.Droid.Contract.AddPersistedFaceResult> AddPersonFace (string mPersonGroupId, UUID mPersonId, Stream mImageStream, string userData, Face.Droid.Contract.FaceRectangle targetFace)
		{
			return Task.Run (() =>
			 {
				 return Client.AddPersonFace (mPersonGroupId, mPersonId, mImageStream, userData, targetFace);
			 });
		}

		public Task<Face.Droid.Contract.GroupResult> Group (UUID [] faceIds)
		{
			return Task.Run (() =>
			 {
				 return Client.Group (faceIds);
			 });
		}
	}
}