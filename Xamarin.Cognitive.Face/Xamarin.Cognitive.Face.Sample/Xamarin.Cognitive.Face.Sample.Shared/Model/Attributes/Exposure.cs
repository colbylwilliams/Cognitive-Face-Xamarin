namespace Xamarin.Cognitive.Face.Shared
{
	public class Exposure : Attribute
	{
		public ExposureLevel ExposureLevel { get; set; }


		public float Value { get; set; }


		public override string ToString ()
		{
			return $"Exposure Level: {ExposureLevel} ({Value})";
		}
	}
}