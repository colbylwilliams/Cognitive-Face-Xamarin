namespace Xamarin.Cognitive.Face.Model
{
    public class Makeup : Attribute
    {
        public bool EyeMakeup { get; set; }

        public bool LipMakeup { get; set; }

        public override string ToString ()
        {
            var list = BuildTypeList (
                ("Eye", EyeMakeup),
                ("Lip", LipMakeup));

            return $"Makeup: {list}";
        }
    }
}