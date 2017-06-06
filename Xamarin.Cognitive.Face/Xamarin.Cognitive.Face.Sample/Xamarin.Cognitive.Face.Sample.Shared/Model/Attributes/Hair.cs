using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Sample.Shared
{
	public class Hair : Attribute
	{
		public float Bald { get; set; }


		public bool Invisible { get; set; }


		public Dictionary<HairColorType, float> HairColor { get; set; } = new Dictionary<HairColorType, float> ();


		public string HairString { get; set; }


		public override string ToString ()
		{
			return $"Hair: {HairString}";
		}
	}
}