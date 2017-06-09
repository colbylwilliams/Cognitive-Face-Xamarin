namespace Xamarin.Cognitive.Face.Shared
{
	public class CandidateResult
	{
		public Person Person { get; set; }

		public string PersonId { get; set; }

		public float Confidence { get; set; }
	}
}