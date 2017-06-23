namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// SimilarFaceResult represents the result of a Find Similar operation.
	/// </summary>
	public class SimilarFaceResult
	{
		/// <summary>
		/// Gets or sets the similar face that was found.
		/// </summary>
		/// <value>The face.</value>
		/// <remarks>This value will only be populated when the calling method can successfully match <see cref="FaceId"/> to an existing or cached <see cref="Face"/></remarks>
		public Face Face { get; set; }


		/// <summary>
		/// Gets or sets the Id of the similar face that was found.
		/// </summary>
		/// <value>The face identifier.</value>
		/// <remarks>This value should always be populated as the result of the Find Similar operation.</remarks>
		public string FaceId { get; set; }


		/// <summary>
		/// Gets or sets the confidence factore for this Find Similar match/result.
		/// </summary>
		/// <value>The confidence.</value>
		public float Confidence { get; set; }
	}
}