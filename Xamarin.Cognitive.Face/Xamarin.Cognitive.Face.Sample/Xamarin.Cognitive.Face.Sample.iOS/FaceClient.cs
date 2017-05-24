using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.Sample.iOS;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample
{
	public partial class FaceClient
	{
		MPOFaceServiceClient client;
		MPOFaceServiceClient Client => client ?? (client = new MPOFaceServiceClient (SubscriptionKey));

		public FaceClient ()
		{
			//TODO: possibly allow the user to switch region API is being called: 

			//			West US -westus.api.cognitive.microsoft.com
			//East US 2 - eastus2.api.cognitive.microsoft.com
			//West Central US - westcentralus.api.cognitive.microsoft.com
			//West Europe -westeurope.api.cognitive.microsoft.com
			//Southeast Asia -southeastasia.api.cognitive.microsoft.com
		}


		#region Groups


		public Task<List<PersonGroup>> GetGroups (bool forceRefresh = false)
		{
			try
			{
				if (Groups.Count == 0 || forceRefresh)
				{
					var tcs = new TaskCompletionSource<List<PersonGroup>> ();

					Client.ListPersonGroupsWithCompletion ((groups, error) =>
					{
						tcs.FailTaskIfErrored (error.ToException ());
						if (tcs.IsNullFinishCanceledOrFaulted ()) return;

						Groups = new List<PersonGroup> (
							groups.Select (g => g.ToPersonGroup ())
						);

						tcs.SetResult (Groups);
					}).Resume ();

					return tcs.Task;
				}

				return Task.FromResult (Groups);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<PersonGroup> CreatePersonGroup (string groupName, string userData = null)
		{
			try
			{
				var tcs = new TaskCompletionSource<PersonGroup> ();

				var personGroupId = Guid.NewGuid ().ToString ();

				Client.CreatePersonGroupWithId (personGroupId, groupName, userData, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var group = new PersonGroup
					{
						Name = groupName,
						Id = personGroupId,
						UserData = userData
					};

					Groups.Add (group);

					tcs.SetResult (group);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task UpdatePersonGroup (PersonGroup personGroup, string groupName, string userData = null)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.UpdatePersonGroupWithPersonGroupId (personGroup.Id, groupName, userData, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					personGroup.Name = groupName;
					personGroup.UserData = userData;

					tcs.SetResult (true);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task DeleteGroup (PersonGroup personGroup)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.DeletePersonGroupWithPersonGroupId (personGroup.Id, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					if (Groups.Contains (personGroup))
					{
						Groups.Remove (personGroup);
					}

					tcs.SetResult (true);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task TrainGroup (PersonGroup personGroup)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.TrainPersonGroupWithPersonGroupId (personGroup.Id, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					tcs.SetResult (true);

				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		#endregion


		#region Person


		public Task<List<Person>> GetPeopleForGroup (PersonGroup group, bool forceRefresh = false)
		{
			try
			{
				if (group.People?.Count > 0 && !forceRefresh)
				{
					return Task.FromResult (group.People);
				}

				var tcs = new TaskCompletionSource<List<Person>> ();

				Client.ListPersonsWithPersonGroupId (group.Id, (mpoPeople, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var people = new List<Person> (
						mpoPeople.Select (p => p.ToPerson ())
					);

					group.People.Clear ();
					group.People.AddRange (people);

					tcs.SetResult (people);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<Person> CreatePerson (string personName, PersonGroup group, string userData = null)
		{
			try
			{
				var tcs = new TaskCompletionSource<Person> ();

				Client.CreatePersonWithPersonGroupId (group.Id, personName, userData, (result, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					tcs.FailTaskByCondition (string.IsNullOrEmpty (result.PersonId), "CreatePersonResult returned invalid person Id");
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var person = new Person
					{
						Name = personName,
						Id = result.PersonId,
						UserData = userData
					};

					group.People.Add (person);

					tcs.SetResult (person);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task UpdatePerson (Person person, PersonGroup group, string personName, string userData = null)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.UpdatePersonWithPersonGroupId (group.Id, person.Id, personName, userData, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					person.Name = personName;
					person.UserData = userData;

					tcs.SetResult (true);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task DeletePerson (Person person, PersonGroup group)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.DeletePersonWithPersonGroupId (group.Id, person.Id, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					if (group.People.Contains (person))
					{
						group.People.Remove (person);
					}

					tcs.SetResult (true);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<Person> GetPerson (PersonGroup group, string personId)
		{
			try
			{
				//if people are already loaded for this group try to find it there...
				if (group.People?.Count > 0)
				{
					var person = group.People.FirstOrDefault (p => p.Id == personId);

					if (person != null)
					{
						return Task.FromResult (person);
					}
				}

				var tcs = new TaskCompletionSource<Person> ();

				Client.GetPersonWithPersonGroupId (group.Id, personId, (mpoPerson, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var person = mpoPerson.ToPerson ();

					//add them to the group?
					group.People.Add (person);

					tcs.SetResult (person);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task AddFaceForPerson (Person person, PersonGroup group, Shared.Face face, UIImage photo, string userData = null, float quality = .8f)
		{
			var tcs = new TaskCompletionSource<bool> ();

			try
			{
				var faceRect = face.FaceRectangle.ToMPOFaceRect ();

				using (var jpgData = photo.AsJPEG (quality))
				{
					Client.AddPersonFaceWithPersonGroupId (group.Id, person.Id, jpgData, userData, faceRect, (result, error) =>
					{
						tcs.FailTaskIfErrored (error.ToException ());
						tcs.FailTaskByCondition (string.IsNullOrEmpty (result?.PersistedFaceId), "AddPersistedFaceResult returned invalid face Id");
						if (tcs.IsNullFinishCanceledOrFaulted ()) return;

						face.Id = result.PersistedFaceId;
						face.UpdatePhotoPath ();

						person.Faces.Add (face);

						face.SavePhotoFromSource (photo);

						tcs.SetResult (true);
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


		public Task DeleteFace (Person person, PersonGroup group, Shared.Face face)
		{
			try
			{
				var tcs = new TaskCompletionSource<bool> ();

				Client.DeletePersonFaceWithPersonGroupId (group.Id, person.Id, face.Id, error =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					if (person.Faces.Contains (face))
					{
						person.Faces.Remove (face);
					}

					tcs.SetResult (true);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public async Task<List<Shared.Face>> GetFacesForPerson (Person person, PersonGroup group)
		{
			try
			{
				person.Faces.Clear ();

				if (person.FaceIds?.Count > 0)
				{
					foreach (var faceId in person.FaceIds)
					{
						var face = await GetFaceForPerson (person, group, faceId);

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


		public Task<Shared.Face> GetFaceForPerson (Person person, PersonGroup group, string persistedFaceId)
		{
			try
			{
				var tcs = new TaskCompletionSource<Shared.Face> ();

				Client.GetPersonFaceWithPersonGroupId (group.Id, person.Id, persistedFaceId, (mpoFace, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var face = mpoFace.ToFace ();

					tcs.SetResult (face);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
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
						tcs.FailTaskIfErrored (error.ToException ());
						if (tcs.IsNullFinishCanceledOrFaulted ()) return;

						foreach (var detectedFace in detectedFaces)
						{
							var face = detectedFace.ToFace ();
							faces.Add (face);

							face.SavePhotoFromSource (photo);
						}

						tcs.SetResult (faces);
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
			try
			{
				var results = new List<IdentificationResult> ();
				var tcs = new TaskCompletionSource<List<IdentificationResult>> ();

				var detectedFaceIds = detectedFaces.Select (f => f.Id).ToArray ();

				Client.IdentifyWithPersonGroupId (group.Id, detectedFaceIds, maxNumberOfCandidates, async (identifyResults, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

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
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<VerifyResult> Verify (Shared.Face face1, Shared.Face face2)
		{
			try
			{
				var tcs = new TaskCompletionSource<VerifyResult> ();

				Client.VerifyWithFirstFaceId (face1.Id, face2.Id, (verifyResult, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var result = new VerifyResult
					{
						IsIdentical = verifyResult.IsIdentical,
						Confidence = verifyResult.Confidence
					};

					tcs.SetResult (result);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<VerifyResult> Verify (Shared.Face face, Person person, PersonGroup personGroup)
		{
			try
			{
				var tcs = new TaskCompletionSource<VerifyResult> ();

				Client.VerifyWithFaceId (face.Id, person.Id, personGroup.Id, (verifyResult, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

					var result = new VerifyResult
					{
						IsIdentical = verifyResult.IsIdentical,
						Confidence = verifyResult.Confidence
					};

					tcs.SetResult (result);
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<List<SimilarFaceResult>> FindSimilar (Shared.Face targetFace, List<Shared.Face> faceList)
		{
			try
			{
				var tcs = new TaskCompletionSource<List<SimilarFaceResult>> ();
				var faceIds = faceList.Select (f => f.Id).ToArray ();
				var results = new List<SimilarFaceResult> ();

				Client.FindSimilarWithFaceId (targetFace.Id, faceIds, (similarFaces, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

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
				}).Resume ();

				return tcs.Task;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				throw;
			}
		}


		public Task<List<FaceGroup>> GroupFaces (List<Shared.Face> targetFaces)
		{
			try
			{
				var tcs = new TaskCompletionSource<List<FaceGroup>> ();
				var faceIds = targetFaces.Select (f => f.Id).ToArray ();
				var results = new List<FaceGroup> ();

				Client.GroupWithFaceIds (faceIds, (groupResult, error) =>
				{
					tcs.FailTaskIfErrored (error.ToException ());
					if (tcs.IsNullFinishCanceledOrFaulted ()) return;

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
				}).Resume ();

				return tcs.Task;
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