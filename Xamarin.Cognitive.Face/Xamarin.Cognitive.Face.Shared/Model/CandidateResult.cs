namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Candidate result is a possible identifcation match in an Identify operation.
	/// </summary>
	public class CandidateResult
	{
		/// <summary>
		/// Gets or sets the person this result is for.
		/// </summary>
		/// <value>The person.</value>
		public Person Person { get; set; }


		/// <summary>
		/// Gets or sets the person Id this result is for.
		/// </summary>
		/// <value>The person identifier.</value>
		public string PersonId { get; set; }


		/// <summary>
		/// Gets or sets the confidence factor of the match.
		/// </summary>
		/// <value>The confidence.</value>
		public float Confidence { get; set; }
	}
}