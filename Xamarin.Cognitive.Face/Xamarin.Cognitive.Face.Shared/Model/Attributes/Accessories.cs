using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// The accessories the detected face was found to contain.
	/// </summary>
	public class Accessories : Attribute
	{
		/// <summary>
		/// Gets a list of all accessories the face has.
		/// </summary>
		/// <value>The accessories list.</value>
		public Dictionary<AccessoryType, float> AccessoriesList { get; } = new Dictionary<AccessoryType, float> ();


		/// <summary>
		/// Gets or sets the string representation of the accessories.
		/// </summary>
		/// <value>The accessories string.</value>
		public string AccessoriesString { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Accessories"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Accessories"/>.</returns>
		public override string ToString ()
		{
			return $"Accessories: {AccessoriesString}";
		}
	}
}