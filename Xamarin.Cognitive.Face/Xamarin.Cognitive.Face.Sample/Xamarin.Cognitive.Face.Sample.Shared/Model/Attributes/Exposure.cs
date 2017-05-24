namespace Xamarin.Cognitive.Face.Sample.Shared
{
    public class Exposure : Attribute
    {
        public string ExposureLevel { get; set; }

        public float Value { get; set; }

        public override string ToString ()
        {
            return $"Exposure Level: {ExposureLevel} ({Value})";
        }
    }
}