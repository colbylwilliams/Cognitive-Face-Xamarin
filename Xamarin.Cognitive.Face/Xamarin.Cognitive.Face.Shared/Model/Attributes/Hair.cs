using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Hair attribute: whether the hair is visible, baldness indicator, and also including hair color if available.
	/// </summary>
	public class Hair : Attribute
	{
		/// <summary>
		/// Gets or sets the baldness value.
		/// </summary>
		/// <value>The bald value.</value>
		public float Bald { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether this the hair is invisible.
		/// </summary>
		/// <value><c>true</c> if invisible; otherwise, <c>false</c>.</value>
		public bool Invisible { get; set; }


		/// <summary>
		/// Gets or sets a Dictionary with hair colors and associated confidence values for each color detected.
		/// </summary>
		/// <value>The color of the hair.</value>
		public Dictionary<HairColorType, float> HairColor { get; set; } = new Dictionary<HairColorType, float> ();


		/// <summary>
		/// Gets or sets a string indicating the hair attributes.
		/// </summary>
		/// <value>The hair string.</value>
		public string HairString { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Hair"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Hair"/>.</returns>
		public override string ToString ()
		{
			return $"Hair: {HairString}";
		}
	}
}