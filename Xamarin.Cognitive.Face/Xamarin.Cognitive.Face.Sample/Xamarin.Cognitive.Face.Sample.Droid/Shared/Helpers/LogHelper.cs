using System;
using System.Collections.Generic;

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
			if (!logs.TryGetValue (logType, out List<string> log))
			{
				log = new List<string> ();
				logs [logType] = log;
			}

			return log;
		}


		public static void AddLog (LogType logType, string msg)
		{
			GetLog (logType).Add (GetLogHeader () + msg);
		}


		public static void ClearLog (LogType logType)
		{
			GetLog (logType).Clear ();
		}


		// Get the current time and add to log.
		static string GetLogHeader ()
		{
			return $"[{DateTime.Now.ToString ("HH:mm:ss")}] ";
		}
	}
}