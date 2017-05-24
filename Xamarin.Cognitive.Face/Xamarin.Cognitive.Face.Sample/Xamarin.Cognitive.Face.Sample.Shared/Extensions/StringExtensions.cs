namespace Xamarin.Cognitive.Face.Sample.Shared.Extensions
{
	public static class StringExtensions
	{
		#region Fmt

		public static string Fmt(this string text, params object[] args) => string.Format(text, args);

		public static string Fmt(this string text, object arg1) => string.Format(text, arg1);

		public static string Fmt(this string text, object arg1, object arg2) => string.Format(text, arg1, arg2);

		public static string Fmt(this string text, object arg1, object arg2, object arg3) => string.Format(text, arg1, arg2, arg3);

		#endregion
	}
}