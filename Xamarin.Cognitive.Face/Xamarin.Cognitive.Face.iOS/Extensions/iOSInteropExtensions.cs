using System;
using Foundation;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static class iOSInteropExtensions
	{
		public static float AsFloatSafe (this NSNumber number, float defaultValue = 0)
		{
			return number?.FloatValue ?? defaultValue;
		}


		public static bool AsBoolSafe (this NSNumber number, bool defaultValue = false)
		{
			return number?.BoolValue ?? defaultValue;
		}


		public static DateTime? AsDateSafe (this string dateString, DateTime? defaultValue = null)
		{
			DateTime? date = defaultValue;

			if (DateTime.TryParse (dateString, out DateTime dt))
			{
				date = dt;
			}

			return date;
		}
	}
}