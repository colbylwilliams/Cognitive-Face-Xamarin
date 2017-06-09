using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Shared
{
	public class Accessories : Attribute
	{
		public Dictionary<AccessoryType, float> AccessoriesList { get; set; } = new Dictionary<AccessoryType, float> ();


		public string AccessoriesString { get; set; }


		public override string ToString ()
		{
			return $"Accessories: {AccessoriesString}";
		}
	}
}