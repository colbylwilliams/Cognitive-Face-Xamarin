using System;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// String extensions.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Converts this string into the specified enum type.
		/// </summary>
		/// <returns>The enum.</returns>
		/// <param name="str">String.</param>
		/// <param name="caseInsensitive">If set to <c>true</c> case insensitive.</param>
		/// <typeparam name="TEnum">The type of enum to convert this string to.</typeparam>
		public static TEnum AsEnum<TEnum> (this string str, bool caseInsensitive = true)
		{
			if (string.IsNullOrEmpty (str))
			{
				return default (TEnum);
			}

			return (TEnum) Enum.Parse (typeof (TEnum), str, caseInsensitive);
		}


		/// <summary>
		/// Returns this string with the first character as lowercase.
		/// </summary>
		/// <returns>The string with its first character to lowercase.</returns>
		/// <param name="str">The string to lowercase-ize.</param>
		public static string FirstCharacterToLower (this string str)
		{
			if (string.IsNullOrEmpty (str) || char.IsLower (str, 0))
				return str;

			return char.ToLowerInvariant (str [0]) + str.Substring (1);
		}
	}
}