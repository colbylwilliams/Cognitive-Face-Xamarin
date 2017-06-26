using System;

namespace Xamarin.Cognitive.Face
{
	/// <summary>
	/// Error detail exception is an exception that wraps a native <see cref="Error"/> returned from the Face API.
	/// </summary>
	public class ErrorDetailException : Exception
	{
		/// <summary>
		/// Gets the error detail.
		/// </summary>
		/// <value>The error detail.</value>
		public Error ErrorDetail { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.Cognitive.Face.Domain.ErrorDetailException"/> class.
		/// </summary>
		/// <param name="error">Error.</param>
		public ErrorDetailException (Error error)
		{
			ErrorDetail = error;
		}
	}
}