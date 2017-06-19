using System;
using System.Collections.Generic;
using System.Linq;
using Java.Util;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static class JavaInteropExtensions
	{
		static readonly DateTime epoch;

		static JavaInteropExtensions ()
		{
			epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}


		public static DateTime ToDateTime (this Date jDate)
		{
			return epoch.AddMilliseconds (jDate.Time);
		}


		public static UUID [] AsUUIDs (this IEnumerable<string> ids)
		{
			return ids.Select (id => id.ToUUID ()).ToArray ();
		}


		public static UUID ToUUID (this string Id)
		{
			return UUID.FromString (Id);
		}


		public static List<string> AsStrings (this IEnumerable<UUID> ids)
		{
			return ids.Select (id => id.ToString ()).ToList ();
		}


		public static TEnum AsEnum<TEnum> (this Java.Lang.Enum jEnum, int offset = 0)
		{
			return (TEnum) Enum.ToObject (typeof (TEnum), jEnum.Ordinal () + offset);
		}


		public static TJEnum AsJavaEnum<TJEnum> (this Enum enumValue, bool lowerCaseLeadingLetter = true)
		where TJEnum : Java.Lang.Enum
		{
			var enumString = enumValue.ToString ();

			if (lowerCaseLeadingLetter)
			{
				enumString = enumString.FirstCharacterToLower ();
			}

			return (TJEnum) Java.Lang.Enum.ValueOf (Java.Lang.Class.FromType (typeof (TJEnum)), enumString);
		}
	}
}