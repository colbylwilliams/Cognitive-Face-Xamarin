using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.iOS.Domain;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face
{
	public partial class FaceClient
	{
		MPOFaceServiceClient client;
		MPOFaceServiceClient Client => client ?? (client = new MPOFaceServiceClient (Endpoint, SubscriptionKey));

		FaceClient () { }


		#region Callbacks & Plumbing


		void ProcessError (NSError error)
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


		void NoopResultCallback (NSError error, TaskCompletionSource<bool> tcs)
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
		}


		void AdaptResultCallback<TInput, TResult> (NSError error, TaskCompletionSource<TResult> tcs, TInput data, Func<TInput, TResult> adapter)
		{
			try
			{
				ProcessError (error);
				tcs.SetResult (adapter (data));
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				tcs.TrySetException (ex);
			}
		}


		void AdaptResultCallback<TInput, TResult, TException> (NSError error, TaskCompletionSource<TResult> tcs, TInput data, Func<TInput, TResult> adapter, Func<TException, bool> exceptionProcessor)
			where TException : Exception
		{
			try
			{
				ProcessError (error);
				tcs.SetResult (adapter (data));
			}
			catch (Exception ex)
			{
				if (ex is TException)
				{
					if (exceptionProcessor ((TException) ex))
					{
						return; //handled
					}
				}

				Log.Error (ex);
				tcs.TrySetException (ex);
			}
		}


		void AdaptListResultCallback<TInput, TResult> (NSError error, TaskCompletionSource<List<TResult>> tcs, TInput [] data, Func<TInput, TResult> adapter)
		{
			try
			{
				ProcessError (error);

				var list = data.Select (d => adapter (d)).ToList ();

				tcs.SetResult (list);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				tcs.TrySetException (ex);
			}
		}


		void AdaptListResultCallback<TInput, TParam, TResult> (NSError error, TaskCompletionSource<List<TResult>> tcs, TInput [] data, Func<TInput, TParam, TResult> adapter, TParam adapterParam)
		{
			try
			{
				ProcessError (error);

				var list = data.Select (d => adapter (d, adapterParam)).ToList ();

				tcs.SetResult (list);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				tcs.TrySetException (ex);
			}
		}


		void AdaptListResultCallback<TInput, TParam1, TParam2, TResult> (NSError error, TaskCompletionSource<List<TResult>> tcs, TInput [] data, Func<TInput, TParam1, TParam2, TResult> adapter, TParam1 adapterParam1, TParam2 adapterParam2)
		{
			try
			{
				ProcessError (error);

				var list = data.Select (d => adapter (d, adapterParam1, adapterParam2)).ToList ();

				tcs.SetResult (list);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				tcs.TrySetException (ex);
			}
		}


		#endregion


		#region Person Group


		internal Task<List<PersonGroup>> GetGroups ()
		{
			var tcs = new TaskCompletionSource<List<PersonGroup>> ();

			Client.ListPersonGroupsWithCompletion (
				(personGroups, error) => AdaptListResultCallback (error, tcs, personGroups, MappingExtensions.ToPersonGroup))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<PersonGroup> GetGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<PersonGroup> ();

			Client.GetPersonGroupWithPersonGroupId (
				personGroupId,
				(personGroup, error) => AdaptResultCallback (error, tcs, personGroup, MappingExtensions.ToPersonGroup))
				  .Resume ();

			return tcs.Task;
		}


		internal Task CreatePersonGroup (string personGroupId, string groupName, string userData)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.CreatePersonGroupWithId (
				personGroupId,
				groupName,
				userData,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Updates a <see cref="PersonGroup"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to update.</param>
		/// <param name="groupName">The updated name of the group.</param>
		/// <param name="userData">An updated custom user data string to store with the group.</param>
		/// <remarks>Note that this method does not perform or respect any caching - any cached group loaded via <see cref="GetPersonGroups(bool)"/> or similar methods will not be affected.</remarks>
		public Task UpdatePersonGroup (string personGroupId, string groupName, string userData = null)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.UpdatePersonGroupWithPersonGroupId (
				personGroupId,
				groupName,
				userData,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Deletes a <see cref="PersonGroup"/> with the given <c>personGroupId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to delete.</param>
		/// <remarks>Note that this method does not perform or respect any caching - any cached group loaded via <see cref="GetPersonGroups(bool)"/> or similar methods will not be affected.</remarks>
		public Task DeletePersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonGroupWithPersonGroupId (
				personGroupId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Trains a <see cref="PersonGroup"/> with the given <c>personGroupId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to train.</param>
		public Task TrainPersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.TrainPersonGroupWithPersonGroupId (
				personGroupId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Gets the <see cref="TrainingStatus"/> for the given <see cref="PersonGroup"/>: notstarted, running, succeeded, failed.
		/// </summary>
		/// <returns>The group training status.</returns>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> to get training status for.</param>
		/// <seealso cref="TrainPersonGroup(string)"/>
		public Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			var tcs = new TaskCompletionSource<TrainingStatus> ();

			Client.GetPersonGroupTrainingStatusWithPersonGroupId (
				personGroupId,
				(trainingStatus, error) => AdaptResultCallback (error, tcs, trainingStatus, MappingExtensions.ToTrainingStatus,
				 	(ErrorDetailException ede) =>
					{
						if (ede.ErrorDetail.Code == ErrorCodes.TrainingStatus.PersonGroupNotTrained)
						{
							tcs.SetResult (TrainingStatus.FromStatus (TrainingStatus.TrainingStatusType.NotStarted));
							return true;
						}

						return false;
					}))
				  .Resume ();

			return tcs.Task;
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
			var tcs = new TaskCompletionSource<List<Person>> ();

			Client.ListPersonsWithPersonGroupId (
				personGroupId,
				(people, error) => AdaptListResultCallback (error, tcs, people, MappingExtensions.ToPerson))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Creates a <see cref="Person"/> with the given name and (optionally) custom user data.
		/// </summary>
		/// <returns>The newly created person's Id.</returns>
		/// <param name="personName">The name of the new person.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> this person will be a part of.</param>
		/// <param name="userData">A custom user data string to store with the person.</param>
		public Task<string> CreatePerson (string personName, string personGroupId, string userData = null)
		{
			var tcs = new TaskCompletionSource<string> ();

			Client.CreatePersonWithPersonGroupId (
				personGroupId,
				personName,
				userData,
				(result, error) => AdaptResultCallback (error, tcs, result, r => r.PersonId))
				  .Resume ();

			return tcs.Task;
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
			var tcs = new TaskCompletionSource<bool> ();

			Client.UpdatePersonWithPersonGroupId (
				personGroupId,
				personId,
				personName,
				userData,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Deletes the <see cref="Person"/> with the given <c>personId</c>.
		/// </summary>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to delete.</param>
		public Task DeletePerson (string personGroupId, string personId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonWithPersonGroupId (
				personGroupId,
				personId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Gets the <see cref="Person"/> with the specified <c>personId</c> and belonging to the <see cref="PersonGroup"/> with the given <c>personGroupId</c>.
		/// </summary>
		/// <returns>The <see cref="Person"/>.</returns>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="personId">The Id of the <see cref="Person"/> to get.</param>
		public Task<Person> GetPerson (string personGroupId, string personId)
		{
			var tcs = new TaskCompletionSource<Person> ();

			Client.GetPersonWithPersonGroupId (
				personGroupId,
				personId,
				(person, error) => AdaptResultCallback (error, tcs, person, MappingExtensions.ToPerson))
				  .Resume ();

			return tcs.Task;
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
			var tcs = new TaskCompletionSource<string> ();

			using (var data = NSData.FromStream (stream))
			{
				Client.AddPersonFaceWithPersonGroupId (
					personGroupId,
					personId,
					data,
					userData,
					face.FaceRectangle.ToMPOFaceRect (),
					(result, error) => AdaptResultCallback (error, tcs, result, r => r.PersistedFaceId))
					  .Resume ();

				return tcs.Task;
			}
		}


		/// <summary>
		/// Deletes a persisted <see cref="Face"/> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="faceId">The Id of a persisted <see cref="Face"/> to delete.</param>
		public Task DeletePersonFace (string personId, string personGroupId, string faceId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonFaceWithPersonGroupId (
				personGroupId,
				personId,
				faceId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Gets a persisted <see cref="Face"/> with the given <c>persistedFaceId</c> for the given <see cref="Person"/>.
		/// </summary>
		/// <param name="personId">The Id of the <see cref="Person"/> the given <see cref="Face"/> belongs to.</param>
		/// <param name="personGroupId">The Id of the <see cref="PersonGroup"/> the person is a part of.</param>
		/// <param name="persistedFaceId">The Id of the persisted <see cref="Face"/> to retrieve.</param>
		public Task<Model.Face> GetFaceForPerson (string personId, string personGroupId, string persistedFaceId)
		{
			var tcs = new TaskCompletionSource<Model.Face> ();

			Client.GetPersonFaceWithPersonGroupId (
				personGroupId,
				personId,
				persistedFaceId,
				(face, error) => AdaptResultCallback (error, tcs, face, MappingExtensions.ToFace))
				  .Resume ();

			return tcs.Task;
		}


		#endregion


		#region Face


		internal Task<List<Model.Face>> DetectFacesInPhotoInternal (Stream photoStream, bool returnLandmarks, FaceAttributeType [] attributes)
		{
			var tcs = new TaskCompletionSource<List<Model.Face>> ();

			var types = attributes.Select (a => a.ToNativeFaceAttributeType ()).ToArray ();

			using (var data = NSData.FromStream (photoStream))
			{
				Client.DetectWithData (
					data,
					true,
					returnLandmarks,
					types,
					(detectedFaces, error) => AdaptListResultCallback (error, tcs, detectedFaces, MappingExtensions.ToFace, returnLandmarks, attributes))
					  .Resume ();
			}

			return tcs.Task;
		}


		/// <summary>
		/// Finds faces similar to a target face in the given list of face Ids.
		/// </summary>
		/// <returns>A list of <see cref="SimilarFaceResult"/> indicating similar face(s) and associated confidence factor.</returns>
		/// <param name="targetFaceId">The Id of the target <see cref="Face"/> to find similar faces to.</param>
		/// <param name="faceIds">The face list containing face Ids to compare to the target Face.</param>
		/// <param name="maxCandidatesReturned">The maximum number of candidate faces to return.</param>
		/// <param name="matchMode">The <see cref="FindSimilarMatchMode"/> to use when comparing - matchFace or matchPerson (default).</param>
		/// <remarks><c>maxCandidatesReturned</c> and <c>matchMode</c> are not currently respsected on iOS due to native SDK limiations.</remarks>
		public Task<List<SimilarFaceResult>> FindSimilar (string targetFaceId, string [] faceIds, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			var tcs = new TaskCompletionSource<List<SimilarFaceResult>> ();

			var results = new List<SimilarFaceResult> ();

			Client.FindSimilarWithFaceId (
				targetFaceId,
				faceIds,
				(similarFaces, error) => AdaptListResultCallback (error, tcs, similarFaces, MappingExtensions.ToSimilarFaceResult))
				  .Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Groups similar faces within the list of target face Ids and returns a <see cref="GroupResult"/> with results of the grouping operation.
		/// </summary>
		/// <returns>A <see cref="GroupResult"/> containing <see cref="FaceGroup"/> groups with similar faces and any leftover/messy group.</returns>
		/// <param name="targetFaceIds">The list of target <see cref="Face"/> Ids to perform a grouping operation on.</param>
		public Task<GroupResult> GroupFaces (string [] targetFaceIds)
		{
			var tcs = new TaskCompletionSource<GroupResult> ();

			Client.GroupWithFaceIds (
				targetFaceIds,
				(groupResult, error) => AdaptResultCallback (error, tcs, groupResult, MappingExtensions.ToGroupResult))
				  .Resume ();

			return tcs.Task;
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
			var results = new List<IdentificationResult> ();
			var tcs = new TaskCompletionSource<List<IdentificationResult>> ();

			Client.IdentifyWithPersonGroupId (
				personGroupId,
				detectedFaceIds,
				maxNumberOfCandidates,
				(identifyResults, error) => AdaptListResultCallback (error, tcs, identifyResults, MappingExtensions.ToIdentificationResult))
				.Resume ();

			return tcs.Task;
		}


		/// <summary>
		/// Verifies that the specified faces with Face Ids belong to the same person.
		/// </summary>
		/// <returns>A <see cref="VerifyResult"/> indicating equivalence, with a confidence factor.</returns>
		/// <param name="face1Id">The Id of the first <see cref="Face"/>.</param>
		/// <param name="face2Id">The Id of the second <see cref="Face"/>.</param>
		public Task<VerifyResult> Verify (string face1Id, string face2Id)
		{
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFirstFaceId (
				face1Id,
				face2Id,
				(verifyResult, error) => AdaptResultCallback (error, tcs, verifyResult, MappingExtensions.ToVerifyResult))
				  .Resume ();

			return tcs.Task;
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
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFaceId (
				faceId,
				personId,
				personGroupId,
				(verifyResult, error) => AdaptResultCallback (error, tcs, verifyResult, MappingExtensions.ToVerifyResult))
				  .Resume ();

			return tcs.Task;
		}


		#endregion
	}
}