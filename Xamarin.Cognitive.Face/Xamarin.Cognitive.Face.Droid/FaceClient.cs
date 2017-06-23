using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Droid;
using System;

namespace Xamarin.Cognitive.Face
{
	public partial class FaceClient
	{
		FaceServiceRestClient client;
		FaceServiceRestClient Client => client ?? (client = new FaceServiceRestClient (Endpoint, SubscriptionKey));

		FaceClient () { }


		#region Task/Adapter plumbing


		Task NoopResultAction (Action innerAction)
		{
			return Task.Run (() =>
			{
				try
				{
					innerAction ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		Task<TResult> AdaptResultAction<TInput, TResult> (Func<TInput> caller, Func<TInput, TResult> adapter)
		{
			return Task.Run (() =>
			{
				try
				{
					var result = caller ();
					return adapter (result);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		Task<TResult> AdaptResultAction<TInput, TResult, TException> (Func<TInput> caller, Func<TInput, TResult> adapter, Func<TException, TResult> exceptionProcessor)
			where TException : Exception
			where TResult : class
		{
			return Task.Run (() =>
			{
				try
				{
					var result = caller ();
					return adapter (result);
				}
				catch (Exception ex)
				{
					if (ex is TException)
					{
						var handledResult = exceptionProcessor ((TException) ex);

						if (handledResult != null)
						{
							return handledResult;
						}
					}

					Log.Error (ex);
					throw;
				}
			});
		}


		Task<List<TResult>> AdaptListResultAction<TInput, TResult> (Func<TInput []> caller, Func<TInput, TResult> adapter)
		{
			return Task.Run (() =>
			{
				try
				{
					var results = caller ();
					return results.Select (g => adapter (g)).ToList ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		Task<List<TResult>> AdaptListResultAction<TInput, TParam1, TParam2, TResult> (Func<TInput []> caller, Func<TInput, TParam1, TParam2, TResult> adapter, TParam1 adapterParam1, TParam2 adapterParam2)
		{
			return Task.Run (() =>
			{
				try
				{
					var results = caller ();
					return results.Select (g => adapter (g, adapterParam1, adapterParam2)).ToList ();
				}
				catch (Exception ex)
				{
					Log.Error (ex);
					throw;
				}
			});
		}


		#endregion


		#region Person Group


		internal Task<List<PersonGroup>> GetGroups ()
		{
			return AdaptListResultAction (() => Client.ListPersonGroups (), MappingExtensions.ToPersonGroup);
		}


		internal Task<PersonGroup> GetGroup (string personGroupId)
		{
			return AdaptResultAction (() => Client.GetPersonGroup (personGroupId), MappingExtensions.ToPersonGroup);
		}


		internal Task CreatePersonGroup (string personGroupId, string groupName, string userData)
		{
			return NoopResultAction (() => Client.CreatePersonGroup (personGroupId, groupName, userData));
		}


		/// <summary>
		/// Updates a PersonGroup with the given name and (optionally) custom user data.
		/// </summary>
		/// <param name="personGroupId">The Id of the PersonGroup to update.</param>
		/// <param name="groupName">The updated name of the group.</param>
		/// <param name="userData">An updated custom user data string to store with the group.</param>
		/// <remarks>Note that this method does not perform or respect any caching - any cached group loaded via <see cref="GetPersonGroups(bool)"/> or similar methods will not be affected.</remarks>
		public Task UpdatePersonGroup (string personGroupId, string groupName, string userData = null)
		{
			return NoopResultAction (() => Client.UpdatePersonGroup (personGroupId, groupName, userData));
		}


		/// <summary>
		/// Deletes a PersonGroup with the given <c>personGroupId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the PersonGroup to delete.</param>
		/// <remarks>Note that this method does not perform or respect any caching - any cached group loaded via <see cref="GetPersonGroups(bool)"/> or similar methods will not be affected.</remarks>
		public Task DeletePersonGroup (string personGroupId)
		{
			return NoopResultAction (() => Client.DeletePersonGroup (personGroupId));
		}


		/// <summary>
		/// Trains a PersonGroup with the given <c>personGroupId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the PersonGroup to train.</param>
		public Task TrainPersonGroup (string personGroupId)
		{
			return NoopResultAction (() => Client.TrainPersonGroup (personGroupId));
		}


		/// <summary>
		/// Gets the <see cref="TrainingStatus"/> for the given <see cref="PersonGroup"/>: notstarted, running, succeeded, failed.
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroupId">The Id or the PersonGroup to get training status for.</param>
		/// <seealso cref="TrainPersonGroup(string)"/>
		public Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			return AdaptResultAction (() => Client.GetPersonGroupTrainingStatus (personGroupId), MappingExtensions.ToTrainingStatus,
				(Droid.Rest.ClientException ce) =>
				{
					if (ce.Error?.Code == ErrorCodes.TrainingStatus.PersonGroupNotTrained)
					{
						return TrainingStatus.FromStatus (TrainingStatus.TrainingStatusType.NotStarted);
					}

					return null;
				});
		}


		#endregion


		#region Person


		/// <summary>
		/// Gets the group's list of <see cref="Person"/>.
		/// </summary>
		/// <returns>A list of <see cref="Person"/> for the group.</returns>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to get people for.</param>
		/// <remarks>Note that this method does not perform or respect any caching.</remarks>
		public Task<List<Person>> GetPeopleForGroup (string personGroupId)
		{
			return AdaptListResultAction (() => Client.ListPersons (personGroupId), MappingExtensions.ToPerson);
		}


		/// <summary>
		/// Creates a <see cref="Person"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <returns>The newly created person's Id.</returns>
		/// <param name="personName">The name of the new person.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> this person will be a part of.</param>
		/// <param name="userData">A custom user data string to store with the person.</param>
		public Task<string> CreatePerson (string personName, string personGroupId, string userData)
		{
			return AdaptResultAction (() => Client.CreatePerson (personGroupId, personName, userData), r => r?.PersonId?.ToString ());
		}


		/// <summary>
		/// Updates a <see cref="Person"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> to update.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> this person is a part of.</param>
		/// <param name="personName">The name of the updated person.</param>
		/// <param name="userData">A custom user data string to store with the person.</param>
		public Task UpdatePerson (string personId, string personGroupId, string personName, string userData)
		{
			return NoopResultAction (() => Client.UpdatePerson (personGroupId, personId.ToUUID (), personName, userData));
		}


		/// <summary>
		/// Deletes the <see cref="Person"/> with the given <c>personId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to delete.</param>
		public Task DeletePerson (string personGroupId, string personId)
		{
			return NoopResultAction (() => Client.DeletePerson (personGroupId, personId.ToUUID ()));
		}


		/// <summary>
		/// Gets the <see cref="Person"/> with the specified <c>personId</c> and belonging to the <see cref="PersonGroup"/> with the given <c>personGroupId</c>.
		/// </summary>
		/// <returns>The <see cref="Person"/>.</returns>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to get.</param>
		public Task<Person> GetPerson (string personGroupId, string personId)
		{
			return AdaptResultAction (() => Client.GetPerson (personGroupId, personId.ToUUID ()), MappingExtensions.ToPerson);
		}


		/// <summary>
		/// Adds (saves/persists) a detected <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> to add a face for.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="face">The detected <see cref="Face"/> to add.  This will typically come from the Detect method.</param>
		/// <param name="stream">A stream to the image data containing the Face.</param>
		/// <param name="userData">A custom user data string to store with the person's Face.</param>
		/// <remarks>The Stream passed in to this method will NOT be disposed and should be handled by the calling client code.  
		/// The image stream provided should be the same original image that was used to detect the Face - the <see cref="FaceRectangle"/> must be valid and should contain the correct face.</remarks>
		/// <seealso cref="DetectFacesInPhoto(Stream, bool, FaceAttributeType [])"/>
		public Task<string> AddFaceForPerson (string personId, string personGroupId, Model.Face face, Stream stream, string userData = null)
		{
			return AdaptResultAction (() => Client.AddPersonFace (personGroupId, personId.ToUUID (), stream, userData, face.FaceRectangle.ToNativeFaceRect ()), r => r?.PersistedFaceId?.ToString ());
		}


		/// <summary>
		/// Deletes a persisted <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="faceId">The Id of a persisted <see cref="Face"/> to delete.</param>
		public Task DeletePersonFace (string personId, string personGroupId, string faceId)
		{
			return NoopResultAction (() => Client.DeletePersonFace (personGroupId, personId.ToUUID (), faceId.ToUUID ()));
		}


		/// <summary>
		/// Gets a persisted <see cref="Face"/> with the given <c>persistedFaceId</c> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="persistedFaceId">The Id of the persisted <see cref="Face"/> to retrieve.</param>
		public Task<Model.Face> GetFaceForPerson (string personId, string personGroupId, string persistedFaceId)
		{
			return AdaptResultAction (() => Client.GetPersonFace (personGroupId, personId.ToUUID (), persistedFaceId.ToUUID ()), MappingExtensions.ToFace);
		}


		#endregion


		#region Face


		internal Task<List<Model.Face>> DetectFacesInPhotoInternal (Stream photoStream, bool returnLandmarks, params FaceAttributeType [] attributes)
		{
			var types = attributes.Select (a => a.AsJavaEnum<FaceServiceClientFaceAttributeType> (false)).ToArray ();

			return AdaptListResultAction (() => Client.Detect (photoStream, true, returnLandmarks, types), MappingExtensions.ToFace, returnLandmarks, attributes);
		}


		/// <summary>
		/// Finds faces similar to a target face in the given list of face Ids.
		/// </summary>
		/// <returns>A list of <see cref="SimilarFaceResult"/> indicating similar face(s) and associated confidence factor.</returns>
		/// <param name="targetFaceId">The Id of the target <see cref="Face"/> to find similar faces to.</param>
		/// <param name="faceIds">The face list containing face Ids to compare to the target Face.</param>
		/// <param name="maxCandidatesReturned">The maximum number of candidate faces to return.</param>
		/// <param name="matchMode">The <see cref="FindSimilarMatchMode"/> to use when comparing - matchFace or matchPerson (default).</param>
		public Task<List<SimilarFaceResult>> FindSimilar (string targetFaceId, string [] faceIds, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			return AdaptListResultAction (() => Client.FindSimilar (targetFaceId.ToUUID (), faceIds.AsUUIDs (), maxCandidatesReturned, matchMode.AsJavaEnum<FaceServiceClientFindSimilarMatchMode> ()), MappingExtensions.ToSimilarFaceResult);
		}


		/// <summary>
		/// Groups similar faces within the list of target face Ids and returns a <see cref="GroupResult"/> with results of the grouping operation.
		/// </summary>
		/// <returns>A <see cref="GroupResult"/> containing <see cref="FaceGroup"/> groups with similar faces and any leftover/messy group.</returns>
		/// <param name="targetFaceIds">The list of target <see cref="Face"/> Ids to perform a grouping operation on.</param>
		public Task<GroupResult> GroupFaces (string [] targetFaceIds)
		{
			return AdaptResultAction (() => Client.Group (targetFaceIds.AsUUIDs ()), MappingExtensions.ToGroupResult);
		}


		/// <summary>
		/// Attempts to Identify the given list of detected face Ids against a trained <see cref="PersonGroup"/> containing 1 or more faces.
		/// </summary>
		/// <returns>A list of <see cref="IdentificationResult"/> containing <see cref="CandidateResult"/> indicating potential identification matches and the confidence factor for the match.</returns>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to identify a <see cref="Face"/> against.</param>
		/// <param name="detectedFaceIds">A list of detected face Ids to use for the identification.</param>
		/// <param name="maxNumberOfCandidates">The max number of candidate matches to return.</param>
		public Task<List<IdentificationResult>> Identify (string personGroupId, string [] detectedFaceIds, int maxNumberOfCandidates = 1)
		{
			return AdaptListResultAction (() => Client.Identity (personGroupId, detectedFaceIds.AsUUIDs (), maxNumberOfCandidates), MappingExtensions.ToIdentificationResult);
		}


		/// <summary>
		/// Verifies that the specified faces with Face Ids belong to the same person.
		/// </summary>
		/// <returns>A <see cref="VerifyResult"/> indicating equivalence, with a confidence factor.</returns>
		/// <param name="face1Id">The Id of the first <see cref="Face"/>.</param>
		/// <param name="face2Id">The Id of the second <see cref="Face"/>.</param>
		public Task<VerifyResult> Verify (string face1Id, string face2Id)
		{
			return AdaptResultAction (() => Client.Verify (face1Id.ToUUID (), face2Id.ToUUID ()), MappingExtensions.ToVerifyResult);
		}


		/// <summary>
		/// Verifies that the given face with <c>faceId</c> belongs to the specified <see cref="Person"/> with <c>personId</c>.
		/// </summary>
		/// <returns>A <see cref="VerifyResult"/> indicating equivalence, with a confidence factor.</returns>
		/// <param name="faceId">The Id of the <see cref="Face"/> to verify.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to verify the face against.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the given person belongs to.</param>
		public Task<VerifyResult> Verify (string faceId, string personId, string personGroupId)
		{
			return AdaptResultAction (() => Client.Verify (faceId.ToUUID (), personGroupId, personId.ToUUID ()), MappingExtensions.ToVerifyResult);
		}


		#endregion
	}
}