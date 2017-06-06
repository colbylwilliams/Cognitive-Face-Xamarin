using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Java.IO;
using Java.Util;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Sample.Droid.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		// Ratio to scale a detected face rectangle, the face rectangle scaled up looks more natural.
		const double FACE_RECT_SCALE_RATIO = 1.3;

		FaceServiceRestClient client;
		FaceServiceRestClient Client => client ?? (client = new FaceServiceRestClient (Endpoint, SubscriptionKey));

		FaceClient () { }


		#region Person Group


		public Task<List<PersonGroup>> GetPersonGroups (bool forceRefresh = false)
		{
			return Task.Run (() =>
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

					return Groups;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task<PersonGroup> GetPersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					var personGroup = Client.GetPersonGroup (personGroupId);

					return personGroup.ToPersonGroup ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					var personGroupId = Guid.NewGuid ().ToString ();

					Client.CreatePersonGroup (personGroupId, groupName, userData);

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
			});
		}


		public Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.UpdatePersonGroup (personGroup.Id, groupName, userData);

					personGroup.Name = groupName;
					personGroup.UserData = userData;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task DeletePersonGroup (string personGroupId)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.DeletePersonGroup (personGroupId);

					RemoveGroup (personGroupId);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task TrainPersonGroup (PersonGroup personGroup)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.TrainPersonGroup (personGroup.Id);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		/// <summary>
		/// Gets the group training status: notstarted, running, succeeded, failed
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroupId">Person group Id.</param>
		public Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
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

					Log.Error (cex);
					throw;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		#endregion


		#region Person


		public Task<List<Person>> GetPeopleForGroup (PersonGroup personGroup, bool forceRefresh = false)
		{
			if (personGroup.People?.Count > 0 && !forceRefresh)
			{
				return Task.FromResult (personGroup.People);
			}

			return Task.Run (() =>
			{
				try
				{
					var arrPeople = Client.ListPersons (personGroup.Id);

					var people = new List<Person> (
						arrPeople.Select (p => p.ToPerson ())
					);

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
			});
		}


		public Task<Person> CreatePerson (string personName, PersonGroup personGroup, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					var result = Client.CreatePerson (personGroup.Id, personName, userData);

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
			});
		}


		public Task UpdatePerson (Person person, PersonGroup personGroup, string personName, string userData = null)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.UpdatePerson (personGroup.Id, person.Id.ToUUID (), personName, userData);

					person.Name = personName;
					person.UserData = userData;
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task DeletePerson (PersonGroup personGroup, Person person)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.DeletePerson (personGroup.Id, person.Id.ToUUID ());

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
			});
		}


		public Task<Person> GetPerson (PersonGroup personGroup, string personId)
		{
			//if people are already loaded for this group try to find it there...
			if (personGroup.PeopleLoaded)
			{
				var person = personGroup.People.FirstOrDefault (p => p.Id == personId);

				if (person != null)
				{
					return Task.FromResult (person);
				}
			}

			return Task.Run (() =>
			{
				try
				{
					var jPerson = Client.GetPerson (personGroup.Id, personId.ToUUID ());
					var person = jPerson.ToPerson ();

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
			});
		}


		public Task AddFaceForPerson (Person person, PersonGroup personGroup, Shared.Face face, Bitmap photo, string userData = null, float quality = .8f)
		{
			return Task.Run (() =>
			{
				try
				{
					using (var jpgStream = photo.AsJpeg ())
					{
						var result = Client.AddPersonFace (personGroup.Id, person.Id.ToUUID (), jpgStream, userData, face.FaceRectangle.ToFaceRect ());

						face.Id = result.PersistedFaceId.ToString ();
					}

					face.UpdatePhotoPath ();
					person.Faces.Add (face);

					face.SavePhotoFromSource (photo);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		public Task DeletePersonFace (Person person, PersonGroup personGroup, Shared.Face face)
		{
			return Task.Run (() =>
			{
				try
				{
					Client.DeletePersonFace (personGroup.Id, person.Id.ToUUID (), face.Id.ToUUID ());

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
			});
		}


		public async Task<List<Shared.Face>> GetFacesForPerson (Person person, PersonGroup personGroup)
		{
			try
			{
				person.Faces.Clear ();

				if (person.FaceIds?.Count > 0)
				{
					foreach (var faceId in person.FaceIds)
					{
						var face = await GetFaceForPerson (person, personGroup, faceId);

						person.Faces.Add (face);
					}

					return person.Faces;
				}

				return default (List<Shared.Face>);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<Shared.Face> GetFaceForPerson (Person person, PersonGroup personGroup, string persistedFaceId)
		{
			return Task.Run (() =>
			{
				try
				{
					var persistedFace = Client.GetPersonFace (personGroup.Id, person.Id.ToUUID (), persistedFaceId.ToUUID ());

					return persistedFace.ToFace ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		#endregion


		#region Face


		public Task<List<Shared.Face>> DetectFacesInPhoto (Bitmap photo, bool returnLandmarks = false, params FaceServiceClientFaceAttributeType [] attributes)
		{
			return DetectFacesInPhoto (photo, .8f, returnLandmarks, attributes);
		}


		public Task<List<Shared.Face>> DetectFacesInPhoto (Bitmap photo, float quality, bool returnLandmarks = false, params FaceServiceClientFaceAttributeType [] attributes)
		{
			return Task.Run (() =>
			{
				try
				{
					using (MemoryStream compressedStream = new MemoryStream ())
					{
						if (!photo.Compress (Bitmap.CompressFormat.Jpeg, (int) (quality * 100), compressedStream))
						{
							throw new Exception ("Unable to compress photo to memory stream");
						}

						compressedStream.Position = 0;

						var detectedFaces = Client.Detect (compressedStream, true, returnLandmarks, attributes);

						var faces = new List<Shared.Face> (detectedFaces.Length);

						foreach (var detectedFace in detectedFaces)
						{
							var face = detectedFace.ToFace ();
							//calculate enlarged face rect
							face.FaceRectangleLarge = detectedFace.FaceRectangle.CalculateFaceRectangle (photo, FACE_RECT_SCALE_RATIO);
							faces.Add (face);

							//face.SavePhotoFromSource (photo);
						}

						return faces;
					}
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
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


		public Task TrainPersonGroup (string mPersonGroupId)
		{
			return Task.Run (() =>
			 {
				 Client.TrainPersonGroup (mPersonGroupId);
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