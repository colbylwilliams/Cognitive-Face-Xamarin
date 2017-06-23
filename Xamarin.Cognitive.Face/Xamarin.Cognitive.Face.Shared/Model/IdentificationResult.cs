using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// A result of an Identify operation.
	/// </summary>
	public class IdentificationResult
	{
		/// <summary>
		/// Gets or sets the Id of the face the identify operation is for.
		/// </summary>
		/// <value>The face identifier.</value>
		public string FaceId { get; set; }


		/// <summary>
		/// Gets or sets the <see cref="Face"/> the identify operation is for.
		/// </summary>
		/// <value>The face.</value>
		public Face Face { get; set; }


		/// <summary>
		/// Gets or sets the candidate results.
		/// </summary>
		/// <value>The candidate results.</value>
		public List<CandidateResult> CandidateResults { get; set; }


		/// <summary>
		/// Gets the first candidate result, if any.
		/// </summary>CandidateResult
		/// <value>The candidate result.</value>
		public CandidateResult CandidateResult => CandidateResults.FirstOrDefault ();


		/// <summary>
		/// Gets a value indicating whether there are any <see cref="CandidateResult"/>.
		/// </summary>
		/// <value><c>true</c> if has candidates; otherwise, <c>false</c>.</value>
		public bool HasCandidates => CandidateResults?.Count > 0;
	}
}