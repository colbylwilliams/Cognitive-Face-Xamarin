using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UIKit;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.Sample.iOS.Domain;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		MPOFaceServiceClient client;
		MPOFaceServiceClient Client => client ?? (client = new MPOFaceServiceClient (Endpoint, SubscriptionKey));

		FaceClient () { }


		void ProcessError (Foundation.NSError error)
		{
			if (error != null)
			{
				if (error.Domain != null)
				{
					const string errorKey = "http response is not success : ";
					var detailsIndex = error.Domain.IndexOf (errorKey, StringComparison.Ordinal) + errorKey.Length;
					var errorJson = error.Domain.Substring (detailsIndex);

					var errorDetail = JsonConvert.DeserializeObject<ErrorDetail> (errorJson);

					throw new ErrorDetailException (errorDetail.Error);
				}

				throw new Exception (error.Description);
			}
		}


		void ThrowConditionalError (bool failureCondition, string error)
		{
			if (failureCondition)
			{
				throw new Exception (error);
			}
		}


		#region Person Group


		public Task<List<PersonGroup>> GetGroups (bool forceRefresh = false)
		{
			if (Groups.Count == 0 || forceRefresh)
			{
				var tcs = new TaskCompletionSource<List<PersonGroup>> ();

				Client.ListPersonGroupsWithCompletion ((groups, error) =>
				{
					try
					{
						ProcessError (error);

						if (tcs.IsNullFinishCanceledOrFaulted ()) return;

						Groups = new List<PersonGroup> (
							groups.Select (g => g.ToPersonGroup ())
						);

						tcs.SetResult (Groups);
					}
					catch (Exception ex)
					{
						Log.Error (ex);
						tcs.TrySetException (ex);
					}
				}).Resume ();

				return tcs.Task;
			}

			return Task.FromResult (Groups);
		}


		public Task<PersonGroup> GetPersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<PersonGroup> ();

			Client.GetPersonGroupWithPersonGroupId (personGroupId, (personGroup, error) =>
			{
				try
				{
					ProcessError (error);

					tcs.SetResult (personGroup.ToPersonGroup ());
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			var tcs = new TaskCompletionSource<PersonGroup> ();
			var personGroupId = Guid.NewGuid ().ToString ();

			Client.CreatePersonGroupWithId (personGroupId, groupName, userData, error =>
			{
				try
				{
					ProcessError (error);

					var personGroup = new PersonGroup
					{
						Name = groupName,
						Id = personGroupId,
						UserData = userData
					};

					Groups.Add (personGroup);

					tcs.SetResult (personGroup);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.UpdatePersonGroupWithPersonGroupId (personGroup.Id, groupName, userData, error =>
			{
				try
				{
					ProcessError (error);

					personGroup.Name = groupName;
					personGroup.UserData = userData;

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task DeletePersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonGroupWithPersonGroupId (personGroupId, error =>
			{
				try
				{
					ProcessError (error);

					RemoveGroup (personGroupId);

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task TrainGroup (PersonGroup personGroup)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.TrainPersonGroupWithPersonGroupId (personGroup.Id, error =>
			{
				try
				{
					ProcessError (error);

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Gets the group training status: notstarted, running, succeeded, failed
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroupId">Person group Id.</param>
		public Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			var tcs = new TaskCompletionSource<TrainingStatus> ();

			Client.GetPersonGroupTrainingStatusWithPersonGroupId (personGroupId, (trainingStatus, error) =>
			{
				try
				{
					ProcessError (error);

					tcs.SetResult (trainingStatus.ToTrainingStatus ());
				}
				catch (ErrorDetailException ede)
				{
					if (ede.ErrorDetail.Code == ErrorCodes.TrainingStatus.PersonGroupNotTrained)
					{
						tcs.SetResult (TrainingStatus.FromStatus (TrainingStatus.TrainingStatusType.NotStarted));
					}
					else tcs.TrySetException (ede);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		#endregion


		#region Person


		public Task<List<Person>> GetPeopleForGroup (PersonGroup personGroup, bool forceRefresh = false)
		{
			if (personGroup.PeopleLoaded && !forceRefresh)
			{
				return Task.FromResult (personGroup.People);
			}

			var tcs = new TaskCompletionSource<List<Person>> ();

			Client.ListPersonsWithPersonGroupId (personGroup.Id, (mpoPeople, error) =>
			{
				try
				{
					ProcessError (error);

					var people = new List<Person> (
						mpoPeople.Select (p => p.ToPerson ())
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

					tcs.SetResult (people);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<Person> CreatePerson (string personName, PersonGroup personGroup, string userData = null)
		{
			var tcs = new TaskCompletionSource<Person> ();

			Client.CreatePersonWithPersonGroupId (personGroup.Id, personName, userData, (result, error) =>
			{
				try
				{
					ProcessError (error);
					ThrowConditionalError (string.IsNullOrEmpty (result.PersonId), "CreatePersonResult returned invalid person Id");

					var person = new Person
					{
						Name = personName,
						Id = result.PersonId,
						UserData = userData
					};

					personGroup.People.Add (person);

					tcs.SetResult (person);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task UpdatePerson (Person person, PersonGroup personGroup, string personName, string userData = null)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.UpdatePersonWithPersonGroupId (personGroup.Id, person.Id, personName, userData, error =>
			{
				try
				{
					ProcessError (error);

					person.Name = personName;
					person.UserData = userData;

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task DeletePerson (PersonGroup personGroup, Person person)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonWithPersonGroupId (personGroup.Id, person.Id, error =>
			{
				try
				{
					ProcessError (error);

					if (personGroup.PeopleLoaded && personGroup.People.Contains (person))
					{
						personGroup.People.Remove (person);
					}

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
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

			var tcs = new TaskCompletionSource<Person> ();

			Client.GetPersonWithPersonGroupId (personGroup.Id, personId, (mpoPerson, error) =>
			{
				try
				{
					ProcessError (error);

					var person = mpoPerson.ToPerson ();

					//add them to the group?
					if (personGroup.PeopleLoaded)
					{
						personGroup.People.Add (person);
					}

					tcs.SetResult (person);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task AddFaceForPerson (Person person, PersonGroup personGroup, Shared.Face face, UIImage photo, string userData = null, float quality = .8f)
		{
			var tcs = new TaskCompletionSource<bool> ();

			try
			{
				var faceRect = face.FaceRectangle.ToMPOFaceRect ();

				using (var jpgData = photo.AsJPEG (quality))
				{
					Client.AddPersonFaceWithPersonGroupId (personGroup.Id, person.Id, jpgData, userData, faceRect, (result, error) =>
					{
						try
						{
							ProcessError (error);
							ThrowConditionalError (string.IsNullOrEmpty (result?.PersistedFaceId), "AddPersistedFaceResult returned invalid face Id");

							face.Id = result.PersistedFaceId;
							face.UpdatePhotoPath ();

							person.Faces.Add (face);

							face.SavePhotoFromSource (photo);

							tcs.SetResult (true);
						}
						catch (Exception ex)
						{
							Log.Error (ex);
							tcs.TrySetException (ex);
						}
					}).Resume ();
				}

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task DeletePersonFace (Person person, PersonGroup personGroup, Shared.Face face)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonFaceWithPersonGroupId (personGroup.Id, person.Id, face.Id, error =>
			{
				try
				{
					ProcessError (error);

					if (person.Faces.Contains (face))
					{
						person.Faces.Remove (face);
					}

					tcs.SetResult (true);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
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
			var tcs = new TaskCompletionSource<Shared.Face> ();

			Client.GetPersonFaceWithPersonGroupId (personGroup.Id, person.Id, persistedFaceId, (mpoFace, error) =>
			{
				try
				{
					ProcessError (error);

					var face = mpoFace.ToFace ();

					tcs.SetResult (face);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		#endregion


		#region Face


		public Task<List<Shared.Face>> DetectFacesInPhoto (UIImage photo, params MPOFaceAttributeType [] attributes)
		{
			return DetectFacesInPhoto (photo, .8f, attributes);
		}


		public Task<List<Shared.Face>> DetectFacesInPhoto (UIImage photo, float quality = .8f, params MPOFaceAttributeType [] attributes)
		{
			try
			{
				List<Shared.Face> faces = new List<Shared.Face> ();
				var tcs = new TaskCompletionSource<List<Shared.Face>> ();

				using (var jpgData = photo.AsJPEG (quality))
				{
					Client.DetectWithData (jpgData, true, true, attributes, (detectedFaces, error) =>
					{
						try
						{
							ProcessError (error);

							foreach (var detectedFace in detectedFaces)
							{
								var face = detectedFace.ToFace ();
								faces.Add (face);

								face.SavePhotoFromSource (photo);
							}

							tcs.SetResult (faces);
						}
						catch (Exception ex)
						{
							Log.Error (ex);
							tcs.TrySetException (ex);
						}
					}).Resume ();
				}

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<IdentificationResult>> Identify (PersonGroup group, Shared.Face face, int maxNumberOfCandidates = 1)
		{
			try
			{
				//ensure people are loaded for this group
				await GetPeopleForGroup (group);

				return await IdentifyInternal (group, new Shared.Face [] { face }, maxNumberOfCandidates);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		Task<List<IdentificationResult>> IdentifyInternal (PersonGroup group, Shared.Face [] detectedFaces, int maxNumberOfCandidates = 1)
		{
			var results = new List<IdentificationResult> ();
			var tcs = new TaskCompletionSource<List<IdentificationResult>> ();

			var detectedFaceIds = detectedFaces.Select (f => f.Id).ToArray ();

			Client.IdentifyWithPersonGroupId (group.Id, detectedFaceIds, maxNumberOfCandidates, async (identifyResults, error) =>
			{
				try
				{
					ProcessError (error);

					foreach (MPOIdentifyResult result in identifyResults)
					{
						var face = detectedFaces.FirstOrDefault (f => f.Id == result.FaceId);

						foreach (MPOCandidate candidate in result.Candidates)
						{
							var person = await GetPerson (group, candidate.PersonId);

							var identifyResult = new IdentificationResult
							{
								Person = person,
								Face = face,
								Confidence = candidate.Confidence
							};

							results.Add (identifyResult);
						}
					}

					tcs.SetResult (results);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<VerifyResult> Verify (Shared.Face face1, Shared.Face face2)
		{
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFirstFaceId (face1.Id, face2.Id, (verifyResult, error) =>
			{
				try
				{
					ProcessError (error);

					var result = new VerifyResult
					{
						IsIdentical = verifyResult.IsIdentical,
						Confidence = verifyResult.Confidence
					};

					tcs.SetResult (result);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<VerifyResult> Verify (Shared.Face face, Person person, PersonGroup personGroup)
		{
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFaceId (face.Id, person.Id, personGroup.Id, (verifyResult, error) =>
			{
				try
				{
					ProcessError (error);

					var result = new VerifyResult
					{
						IsIdentical = verifyResult.IsIdentical,
						Confidence = verifyResult.Confidence
					};

					tcs.SetResult (result);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<List<SimilarFaceResult>> FindSimilar (Shared.Face targetFace, List<Shared.Face> faceList)
		{
			var tcs = new TaskCompletionSource<List<SimilarFaceResult>> ();
			var faceIds = faceList.Select (f => f.Id).ToArray ();
			var results = new List<SimilarFaceResult> ();

			Client.FindSimilarWithFaceId (targetFace.Id, faceIds, (similarFaces, error) =>
			{
				try
				{
					ProcessError (error);

					foreach (var similarFace in similarFaces)
					{
						var face = faceList.FirstOrDefault (f => f.Id == similarFace.FaceId);

						results.Add (new SimilarFaceResult
						{
							Face = face,
							FaceId = similarFace.FaceId,
							Confidence = similarFace.Confidence
						});
					}

					tcs.SetResult (results);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		public Task<List<FaceGroup>> GroupFaces (List<Shared.Face> targetFaces)
		{
			var tcs = new TaskCompletionSource<List<FaceGroup>> ();
			var faceIds = targetFaces.Select (f => f.Id).ToArray ();
			var results = new List<FaceGroup> ();

			Client.GroupWithFaceIds (faceIds, (groupResult, error) =>
			{
				try
				{
					ProcessError (error);

					for (var i = 0; i < groupResult.Groups.Count; i++)
					{
						var faceGroup = groupResult.Groups [i];

						results.Add (new FaceGroup
						{
							Title = $"Face Group #{i + 1}",
							Faces = targetFaces.Where (f => faceGroup.Contains (f.Id)).ToList ()
						});
					}

					if (groupResult.MesseyGroup.Length > 0)
					{
						results.Add (new FaceGroup
						{
							Title = "Messy Group",
							Faces = targetFaces.Where (f => groupResult.MesseyGroup.Contains (f.Id)).ToList ()
						});
					}

					tcs.SetResult (results);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					tcs.TrySetException (ex);
				}
			}).Resume ();

			return tcs.Task;
		}


		#endregion
	}
}