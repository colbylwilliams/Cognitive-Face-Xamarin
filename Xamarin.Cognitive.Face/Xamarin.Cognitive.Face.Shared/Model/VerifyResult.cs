namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Represents a results returned from the Face API's Verify function.
	/// </summary>
	public class VerifyResult
	{
		/// <summary>
		/// Gets or sets a value indicating whether the face(s) belong to the person.
		/// </summary>
		/// <value><c>true</c> if is identical; otherwise, <c>false</c>.</value>
		public bool IsIdentical { get; set; }


		/// <summary>
		/// Gets or sets the confidence factor for the <see cref="IsIdentical"/> determination.
		/// </summary>
		/// <value>The confidence.</value>
		public float Confidence { get; set; }
	}
}