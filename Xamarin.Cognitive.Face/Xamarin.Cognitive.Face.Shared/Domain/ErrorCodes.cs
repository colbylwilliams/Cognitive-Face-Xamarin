namespace Xamarin.Cognitive.Face
{
	/// <summary>
	/// Error codes returned from the Face API.
	/// </summary>
	public static class ErrorCodes
	{
		/// <summary>
		/// Training status operation error codes.
		/// </summary>
		public static class TrainingStatus
		{
			/// <summary>
			/// The person group was not found.
			/// </summary>
			public const string PersonGroupNotFound = "PersonGroupNotFound";


			/// <summary>
			/// The person group not trained.
			/// </summary>
			public const string PersonGroupNotTrained = "PersonGroupNotTrained";
		}
	}
}