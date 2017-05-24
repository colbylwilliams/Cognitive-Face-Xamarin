namespace Xamarin.Cognitive.Face.Sample.Shared
{
    public class Accessories : Attribute
    {
        public object [] AccessoriesArray { get; set; }

        public string AccessoriesString { get; set; }

        public override string ToString ()
        {
            return $"Accessories: {AccessoriesString}";
        }
    }
}