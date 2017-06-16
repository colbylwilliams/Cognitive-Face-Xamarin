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


		#region Callback Plumbing


		void NoopResultCallback (NSError error, TaskCompletionSource<bool> tcs)
		{
			try
			{
				ProcessError (error);
				tcs.SetResult (true);
			}
			catch (Exception ex)
			{
				tcs.TrySetException (ex);
			}
		}


		void AdaptResultResultCallback<TInput, TResult> (NSError error, TaskCompletionSource<TResult> tcs, TInput data, Func<TInput, TResult> adapter)
		{
			try
			{
				ProcessError (error);
				tcs.SetResult (adapter (data));
			}
			catch (Exception ex)
			{
				tcs.TrySetException (ex);
			}
		}


		void AdaptResultResultCallback<TInput, TResult, TException> (NSError error, TaskCompletionSource<TResult> tcs, TInput data, Func<TInput, TResult> adapter, Func<TException, bool> exceptionProcessor)
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
				tcs.TrySetException (ex);
			}
		}


		#endregion


		#region Person Group


		internal Task<List<PersonGroup>> GetGroups ()
		{
			var tcs = new TaskCompletionSource<List<PersonGroup>> ();

			Client.ListPersonGroupsWithCompletion (
				(personGroups, error) => AdaptListResultCallback (error, tcs, personGroups, FaceExtensions.ToPersonGroup))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<PersonGroup> GetGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<PersonGroup> ();

			Client.GetPersonGroupWithPersonGroupId (
				personGroupId,
				(personGroup, error) => AdaptResultResultCallback (error, tcs, personGroup, FaceExtensions.ToPersonGroup))
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


		internal Task UpdatePersonGroup (string personGroupId, string groupName, string userData = null)
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


		internal Task DeletePersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonGroupWithPersonGroupId (
				personGroupId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		internal Task TrainPersonGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.TrainPersonGroupWithPersonGroupId (
				personGroupId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<TrainingStatus> GetGroupTrainingStatus (string personGroupId)
		{
			var tcs = new TaskCompletionSource<TrainingStatus> ();

			Client.GetPersonGroupTrainingStatusWithPersonGroupId (
				personGroupId,
				(trainingStatus, error) => AdaptResultResultCallback (error, tcs, trainingStatus, FaceExtensions.ToTrainingStatus,
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


		internal Task<List<Person>> GetPeopleForGroup (string personGroupId)
		{
			var tcs = new TaskCompletionSource<List<Person>> ();

			Client.ListPersonsWithPersonGroupId (
				personGroupId,
				(people, error) => AdaptListResultCallback (error, tcs, people, FaceExtensions.ToPerson))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<string> CreatePerson (string personName, string personGroupId, string userData)
		{
			var tcs = new TaskCompletionSource<string> ();

			Client.CreatePersonWithPersonGroupId (
				personGroupId,
				personName,
				userData,
				(result, error) => AdaptResultResultCallback (error, tcs, result, r => r.PersonId))
				  .Resume ();

			return tcs.Task;
		}


		internal Task UpdatePerson (string personId, string personGroupId, string personName, string userData)
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


		internal Task DeletePerson (string personGroupId, string personId)
		{
			var tcs = new TaskCompletionSource<bool> ();

			Client.DeletePersonWithPersonGroupId (
				personGroupId,
				personId,
				error => NoopResultCallback (error, tcs))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<Person> GetPerson (string personGroupId, string personId)
		{
			var tcs = new TaskCompletionSource<Person> ();

			Client.GetPersonWithPersonGroupId (
				personGroupId,
				personId,
				(person, error) => AdaptResultResultCallback (error, tcs, person, FaceExtensions.ToPerson))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<string> AddFaceForPerson (string personId, string personGroupId, Model.Face face, Stream photoStream, string userData = null)
		{
			var tcs = new TaskCompletionSource<string> ();

			using (var data = NSData.FromStream (photoStream))
			{
				Client.AddPersonFaceWithPersonGroupId (
					personGroupId,
					personId,
					data,
					userData,
					face.FaceRectangle.ToMPOFaceRect (),
					(result, error) => AdaptResultResultCallback (error, tcs, result, r => r.PersistedFaceId))
					  .Resume ();

				return tcs.Task;
			}
		}


		internal Task DeletePersonFace (string personId, string personGroupId, string faceId)
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


		internal Task<Model.Face> GetFaceForPerson (string personId, string personGroupId, string persistedFaceId)
		{
			var tcs = new TaskCompletionSource<Model.Face> ();

			Client.GetPersonFaceWithPersonGroupId (
				personGroupId,
				personId,
				persistedFaceId,
				(face, error) => AdaptResultResultCallback (error, tcs, face, FaceExtensions.ToFace))
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
					(detectedFaces, error) => AdaptListResultCallback (error, tcs, detectedFaces, FaceExtensions.ToFace, returnLandmarks, attributes))
					  .Resume ();
			}

			return tcs.Task;
		}


		internal Task<List<SimilarFaceResult>> FindSimilarInternal (string targetFaceId, string [] faceIds, int maxCandidatesReturned = 1, FindSimilarMatchMode matchMode = FindSimilarMatchMode.MatchPerson)
		{
			var tcs = new TaskCompletionSource<List<SimilarFaceResult>> ();

			var results = new List<SimilarFaceResult> ();

			Client.FindSimilarWithFaceId (
				targetFaceId,
				faceIds,
				(similarFaces, error) => AdaptListResultCallback (error, tcs, similarFaces, FaceExtensions.ToSimilarFaceResult))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<GroupResult> GroupFaces (string [] targetFaceIds)
		{
			var tcs = new TaskCompletionSource<GroupResult> ();

			Client.GroupWithFaceIds (
				targetFaceIds,
				(groupResult, error) => AdaptResultResultCallback (error, tcs, groupResult, FaceExtensions.ToGroupResult))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<List<IdentificationResult>> Identify (string personGroupId, string [] detectedFaceIds, int maxNumberOfCandidates = 1)
		{
			var results = new List<IdentificationResult> ();
			var tcs = new TaskCompletionSource<List<IdentificationResult>> ();

			Client.IdentifyWithPersonGroupId (
				personGroupId,
				detectedFaceIds,
				maxNumberOfCandidates,
				(identifyResults, error) => AdaptListResultCallback (error, tcs, identifyResults, FaceExtensions.ToIdentificationResult))
				.Resume ();

			return tcs.Task;
		}


		internal Task<VerifyResult> Verify (string face1Id, string face2Id)
		{
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFirstFaceId (
				face1Id,
				face2Id,
				(verifyResult, error) => AdaptResultResultCallback (error, tcs, verifyResult, FaceExtensions.ToVerifyResult))
				  .Resume ();

			return tcs.Task;
		}


		internal Task<VerifyResult> Verify (string faceId, string personId, string personGroupId)
		{
			var tcs = new TaskCompletionSource<VerifyResult> ();

			Client.VerifyWithFaceId (
				faceId,
				personId,
				personGroupId,
				(verifyResult, error) => AdaptResultResultCallback (error, tcs, verifyResult, FaceExtensions.ToVerifyResult))
				  .Resume ();

			return tcs.Task;
		}


		#endregion
	}
}