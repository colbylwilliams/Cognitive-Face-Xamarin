using System;

namespace Xamarin.Cognitive.Face.Sample.iOS.Domain
{
	public class ErrorDetailException : Exception
	{
		public Error ErrorDetail { get; private set; }

		public ErrorDetailException (Error error)
		{
			ErrorDetail = error;
		}
	}
}