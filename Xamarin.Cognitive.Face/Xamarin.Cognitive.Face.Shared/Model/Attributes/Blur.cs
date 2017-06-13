namespace Xamarin.Cognitive.Face.Model
{
	public class Blur : Attribute
	{
		public BlurLevel BlurLevel { get; set; }


		public float Value { get; set; }


		public override string ToString ()
		{
			return $"Blur Level: {BlurLevel} ({Value})";
		}
	}
}