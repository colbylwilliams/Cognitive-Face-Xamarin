namespace Xamarin.Cognitive.Face.Model
{
	public class FacialHair : Attribute
	{
		public float Mustache { get; set; }

		public float Beard { get; set; }

		public float Sideburns { get; set; }

		public override string ToString ()
		{
			var list = BuildTypeList (
				("Mustache", Mustache),
				("Beard", Beard),
				("Sideburns", Sideburns)
			);

			return $"Facial Hair: {list}";
		}
	}
}