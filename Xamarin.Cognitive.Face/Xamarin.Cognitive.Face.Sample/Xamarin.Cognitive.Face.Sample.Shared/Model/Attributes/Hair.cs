namespace Xamarin.Cognitive.Face.Sample.Shared
{
    public class Hair : Attribute
    {
        public float Bald { get; set; }

        public float Invisible { get; set; }

        //public Dictionary<string, float> HairColor { get; set; }

        public string HairString { get; set; }

        public override string ToString ()
        {
            return $"Hair: {HairString}";
        }
    }
}