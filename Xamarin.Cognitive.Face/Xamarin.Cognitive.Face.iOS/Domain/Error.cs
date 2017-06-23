namespace Xamarin.Cognitive.Face.iOS.Domain
{
	/// <summary>
	/// Error.
	/// </summary>
	public class Error
	{
		/// <summary>
		/// Gets or sets the error code.
		/// </summary>
		/// <value>The code.</value>
		public string Code { get; set; }


		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }
	}


	/// <summary>
	/// Error detail.
	/// </summary>
	public class ErrorDetail
	{
		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>The error.</value>
		public Error Error { get; set; }
	}
}