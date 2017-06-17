using System.Collections.Generic;
using Android.Icu.Text;
using Java.Util;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	public enum LogType
	{
		General,
		Admin,
		Detection,
		Verification,
		Grouping,
		FindSimilarFaces,
		Identification
	}


	public static class LogHelper
	{
		static Dictionary<LogType, List<string>> logs = new Dictionary<LogType, List<string>> ();

		public static List<string> GetLog (LogType logType)
		{
			return logs [logType];
		}


		public static void AddLog (LogType logType, string msg)
		{
			logs [logType].Add (GetLogHeader () + msg);
		}


		public static void ClearLog (LogType logType)
		{
			logs [logType].Clear ();
		}


		// Get the current time and add to log.
		static string GetLogHeader ()
		{
			var dateFormat = new SimpleDateFormat ("HH:mm:ss", Locale.Us);

			return "[" + dateFormat.Format (Calendar.GetInstance (Locale.Us).Time) + "] ";
		}
	}
}