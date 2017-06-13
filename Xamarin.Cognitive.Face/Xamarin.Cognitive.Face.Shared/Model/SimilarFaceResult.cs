namespace Xamarin.Cognitive.Face.Model
{
	public class SimilarFaceResult
	{
		public Face Face { get; set; }

		public string FaceId { get; set; }

		public float Confidence { get; set; }
	}
}