using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Cognitive.Face.Shared
{
	public class IdentificationResult
	{
		public string FaceId { get; set; }

		public Face Face { get; set; }

		public List<CandidateResult> CandidateResults { get; set; }

		public CandidateResult CandidateResult => CandidateResults.FirstOrDefault ();

		public bool HasCandidates => CandidateResults?.Count > 0;
	}
}