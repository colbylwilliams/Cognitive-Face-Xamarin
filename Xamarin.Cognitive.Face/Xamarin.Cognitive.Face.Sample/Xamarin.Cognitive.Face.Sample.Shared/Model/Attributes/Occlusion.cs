namespace Xamarin.Cognitive.Face.Sample.Shared
{
    public class Occlusion : Attribute
    {
        public bool ForeheadOccluded { get; set; }

        public bool EyeOccluded { get; set; }

        public bool MouthOccluded { get; set; }

        public override string ToString ()
        {
            var list = BuildTypeList (
                ("Eye", EyeOccluded),
                ("Mouth", MouthOccluded),
                ("Forehead", ForeheadOccluded));

            return $"Occlusion: {list}";
        }
    }
}