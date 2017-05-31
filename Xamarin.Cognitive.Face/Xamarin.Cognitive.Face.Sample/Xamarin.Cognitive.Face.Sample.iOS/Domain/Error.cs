namespace Xamarin.Cognitive.Face.Sample.iOS.Domain
{
	public class Error
	{
		public string Code { get; set; }

		public string Message { get; set; }
	}


	public class ErrorDetail
	{
		public Error Error { get; set; }
	}
}