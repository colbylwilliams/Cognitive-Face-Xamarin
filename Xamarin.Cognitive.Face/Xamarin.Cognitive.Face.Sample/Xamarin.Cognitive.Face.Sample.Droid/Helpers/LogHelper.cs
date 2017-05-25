using System;
using System.Collections.Generic;
using Android.Icu.Text;
using Java.Util;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	public static class LogHelper
	{
		// Detection log items.
		private static List<string> mDetectionLog = new List<string> ();

		// Verification log items.
		private static List<string> mVerificationLog = new List<string> ();

		// Grouping log items.
		private static List<string> mGroupingLog = new List<string> ();

		// Find Similar face log items.
		private static List<string> mFindSimilarFaceLog = new List<string> ();

		// Identification log items.
		private static List<string> mIdentificationLog = new List<string> ();

		public static List<String> GetDetectionLog ()
		{
			return mDetectionLog;
		}

		// Add a new detection log item.
		public static void AddDetectionLog (String log)
		{
			mDetectionLog.Add (LogHelper.GetLogHeader () + log);
		}

		// Clear all detection log items.
		public static void ClearDetectionLog ()
		{
			mDetectionLog.Clear ();
		}

		// Get all the verification log items.
		public static List<String> GetVerificationLog ()
		{
			return mVerificationLog;
		}

		// Add a new verification log item.
		public static void AddVerificationLog (String log)
		{
			mVerificationLog.Add (LogHelper.GetLogHeader () + log);
		}

		// Clear all verification log items.
		public static void ClearVerificationLog ()
		{
			mVerificationLog.Clear ();
		}

		// Get all the grouping log items.
		public static List<String> GetGroupingLog ()
		{
			return mGroupingLog;
		}

		// Add a new grouping log item.
		public static void AddGroupingLog (String log)
		{
			mGroupingLog.Add (LogHelper.GetLogHeader () + log);
		}

		// Clear all grouping log items.
		public static void ClearGroupingLog ()
		{
			mGroupingLog.Clear ();
		}

		// Get all the find similar face log items.
		public static List<String> GetFindSimilarFaceLog ()
		{
			return mFindSimilarFaceLog;
		}

		// Add a new find similar face log item.
		public static void AddFindSimilarFaceLog (String log)
		{
			mFindSimilarFaceLog.Add (LogHelper.GetLogHeader () + log);
		}

		// Clear all find similar face log items.
		public static void ClearFindSimilarFaceLog ()
		{
			mFindSimilarFaceLog.Clear ();
		}

		// Get all the identification log items.
		public static List<String> GetIdentificationLog ()
		{
			return mIdentificationLog;
		}

		// Add a new identification log item.
		public static void AddIdentificationLog (String log)
		{
			mIdentificationLog.Add (LogHelper.GetLogHeader () + log);
		}

		// Clear all identification log items.
		public static void ClearIdentificationLog ()
		{
			mIdentificationLog.Clear ();
		}

		// Get the current time and add to log.
		private static String GetLogHeader ()
		{
			DateFormat dateFormat = new SimpleDateFormat ("HH:mm:ss", Locale.Us);
			return "[" + dateFormat.Format (Java.Util.Calendar.GetInstance (Locale.Us).Time) + "] ";
		}
	}
}