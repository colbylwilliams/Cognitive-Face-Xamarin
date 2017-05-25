using System;
using System.Collections.Generic;
using Android.Content;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	public static class StorageHelper
	{
		public static ICollection<String> GetAllPersonGroupIds (Context context)
		{
			ISharedPreferences personGroupIdSet =
					context.GetSharedPreferences ("PersonGroupIdSet", FileCreationMode.Private);

			return personGroupIdSet.GetStringSet ("PersonGroupIdSet", new HashSet<String> ());
		}

		public static String GetPersonGroupName (String personGroupId, Context context)
		{
			ISharedPreferences personGroupIdNameMap =
					context.GetSharedPreferences ("PersonGroupIdNameMap", FileCreationMode.Private);

			return personGroupIdNameMap.GetString (personGroupId, "");
		}

		public static void SetPersonGroupName (String personGroupIdToAdd, String personGroupName, Context context)
		{
			ISharedPreferences personGroupIdNameMap =
					context.GetSharedPreferences ("PersonGroupIdNameMap", FileCreationMode.Private);

			ISharedPreferencesEditor personGroupIdNameMapEditor = personGroupIdNameMap.Edit ();
			personGroupIdNameMapEditor.PutString (personGroupIdToAdd, personGroupName);
			personGroupIdNameMapEditor.Commit ();

			ICollection<String> personGroupIds = GetAllPersonGroupIds (context);
			ICollection<String> newPersonGroupIds = new HashSet<String> ();

			foreach (String personGroupId in personGroupIds)
			{
				newPersonGroupIds.Add (personGroupId);
			}

			newPersonGroupIds.Add (personGroupIdToAdd);
			ISharedPreferences personGroupIdSet =
					context.GetSharedPreferences ("PersonGroupIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor personGroupIdSetEditor = personGroupIdSet.Edit ();
			personGroupIdSetEditor.PutStringSet ("PersonGroupIdSet", newPersonGroupIds);
			personGroupIdSetEditor.Commit ();
		}

		public static void DeletePersonGroups (List<String> personGroupIdsToDelete, Context context)
		{
			ISharedPreferences personGroupIdNameMap =
					context.GetSharedPreferences ("PersonGroupIdNameMap", FileCreationMode.Private);
			ISharedPreferencesEditor personGroupIdNameMapEditor = personGroupIdNameMap.Edit ();

			foreach (String personGroupId in personGroupIdsToDelete)
			{
				personGroupIdNameMapEditor.Remove (personGroupId);
			}

			personGroupIdNameMapEditor.Commit ();

			ICollection<String> personGroupIds = GetAllPersonGroupIds (context);
			ICollection<String> newPersonGroupIds = new HashSet<String> ();

			foreach (String personGroupId in personGroupIds)
			{
				if (!personGroupIdsToDelete.Contains (personGroupId))
				{
					newPersonGroupIds.Add (personGroupId);
				}
			}

			ISharedPreferences personGroupIdSet =
					context.GetSharedPreferences ("PersonGroupIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor personGroupIdSetEditor = personGroupIdSet.Edit ();
			personGroupIdSetEditor.PutStringSet ("PersonGroupIdSet", newPersonGroupIds);
			personGroupIdSetEditor.Commit ();
		}

		public static ICollection<String> GetAllPersonIds (String personGroupId, Context context)
		{
			ISharedPreferences personIdSet =
					context.GetSharedPreferences (personGroupId + "PersonIdSet", FileCreationMode.Private);

			return personIdSet.GetStringSet ("PersonIdSet", new HashSet<String> ());
		}

		public static String GetPersonName (String personId, String personGroupId, Context context)
		{
			ISharedPreferences personIdNameMap =
					context.GetSharedPreferences (personGroupId + "PersonIdNameMap", FileCreationMode.Private);

			return personIdNameMap.GetString (personId, "");
		}

		public static void SetPersonName (String personIdToAdd, String personName, String personGroupId, Context context)
		{
			ISharedPreferences personIdNameMap =
					context.GetSharedPreferences (personGroupId + "PersonIdNameMap", FileCreationMode.Private);

			ISharedPreferencesEditor personIdNameMapEditor = personIdNameMap.Edit ();
			personIdNameMapEditor.PutString (personIdToAdd, personName);
			personIdNameMapEditor.Commit ();

			ICollection<String> personIds = GetAllPersonIds (personGroupId, context);
			ICollection<String> newPersonIds = new HashSet<String> ();

			foreach (String personId in personIds)
			{
				newPersonIds.Add (personId);
			}

			newPersonIds.Add (personIdToAdd);
			ISharedPreferences personIdSet =
					context.GetSharedPreferences (personGroupId + "PersonIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor personIdSetEditor = personIdSet.Edit ();
			personIdSetEditor.PutStringSet ("PersonIdSet", newPersonIds);
			personIdSetEditor.Commit ();
		}

		public static void DeletePersons (List<String> personIdsToDelete, String personGroupId, Context context)
		{
			ISharedPreferences personIdNameMap =
					context.GetSharedPreferences (personGroupId + "PersonIdNameMap", FileCreationMode.Private);
			ISharedPreferencesEditor personIdNameMapEditor = personIdNameMap.Edit ();

			foreach (String personId in personIdsToDelete)
			{
				personIdNameMapEditor.Remove (personId);
			}

			personIdNameMapEditor.Commit ();

			ICollection<String> personIds = GetAllPersonIds (personGroupId, context);
			ICollection<String> newPersonIds = new HashSet<String> ();

			foreach (String personId in personIds)
			{
				if (!personIdsToDelete.Contains (personId))
				{
					newPersonIds.Add (personId);
				}
			}

			ISharedPreferences personIdSet =
					context.GetSharedPreferences (personGroupId + "PersonIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor personIdSetEditor = personIdSet.Edit ();
			personIdSetEditor.PutStringSet ("PersonIdSet", newPersonIds);
			personIdSetEditor.Commit ();
		}

		public static ICollection<String> GetAllFaceIds (String personId, Context context)
		{
			ISharedPreferences faceIdSet =
					context.GetSharedPreferences (personId + "FaceIdSet", FileCreationMode.Private);

			return faceIdSet.GetStringSet ("FaceIdSet", new HashSet<String> ());
		}

		public static String GetFaceUri (String faceId, Context context)
		{
			ISharedPreferences faceIdUriMap =
					context.GetSharedPreferences ("FaceIdUriMap", FileCreationMode.Private);

			return faceIdUriMap.GetString (faceId, "");
		}

		public static void SetFaceUri (String faceIdToAdd, String faceUri, String personId, Context context)
		{
			ISharedPreferences faceIdUriMap =
					context.GetSharedPreferences ("FaceIdUriMap", FileCreationMode.Private);

			ISharedPreferencesEditor faceIdUriMapEditor = faceIdUriMap.Edit ();
			faceIdUriMapEditor.PutString (faceIdToAdd, faceUri);
			faceIdUriMapEditor.Commit ();

			ICollection<String> faceIds = GetAllFaceIds (personId, context);
			ICollection<String> newFaceIds = new HashSet<String> ();

			foreach (String faceId in faceIds)
			{
				newFaceIds.Add (faceId);
			}

			newFaceIds.Add (faceIdToAdd);
			ISharedPreferences faceIdSet =
					context.GetSharedPreferences (personId + "FaceIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor faceIdSetEditor = faceIdSet.Edit ();
			faceIdSetEditor.PutStringSet ("FaceIdSet", newFaceIds);
			faceIdSetEditor.Commit ();
		}

		public static void DeleteFaces (List<String> faceIdsToDelete, String personId, Context context)
		{
			ICollection<String> faceIds = GetAllFaceIds (personId, context);
			ICollection<String> newFaceIds = new HashSet<String> ();

			foreach (String faceId in faceIds)
			{
				if (!faceIdsToDelete.Contains (faceId))
				{
					newFaceIds.Add (faceId);
				}
			}

			ISharedPreferences faceIdSet =
					context.GetSharedPreferences (personId + "FaceIdSet", FileCreationMode.Private);
			ISharedPreferencesEditor faceIdSetEditor = faceIdSet.Edit ();
			faceIdSetEditor.PutStringSet ("FaceIdSet", newFaceIds);
			faceIdSetEditor.Commit ();
		}
	}
}