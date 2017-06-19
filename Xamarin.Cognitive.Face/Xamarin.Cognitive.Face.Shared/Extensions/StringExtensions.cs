using System;

namespace Xamarin.Cognitive.Face.Extensions
{
	public static class StringExtensions
	{
		public static TEnum AsEnum<TEnum> (this string str, bool caseInsensitive = true)
		{
			if (string.IsNullOrEmpty (str))
			{
				return default (TEnum);
			}

			return (TEnum) Enum.Parse (typeof (TEnum), str, caseInsensitive);
		}


		public static string FirstCharacterToLower (this string str)
		{
			if (string.IsNullOrEmpty (str) || char.IsLower (str, 0))
				return str;

			return char.ToLowerInvariant (str [0]) + str.Substring (1);
		}
	}
}