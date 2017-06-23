namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Makeup: Whether face area (eye, lip) is made-up or not.
	/// </summary>
	public class Makeup : Attribute
	{
		/// <summary>
		/// Gets or sets a value indicating whether the face has eye makeup.
		/// </summary>
		/// <value><c>true</c> if eye makeup; otherwise, <c>false</c>.</value>
		public bool EyeMakeup { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the face has lip makeup.
		/// </summary>
		/// <value><c>true</c> if lip makeup; otherwise, <c>false</c>.</value>
		public bool LipMakeup { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Makeup"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Makeup"/>.</returns>
		public override string ToString ()
		{
			var list = BuildTypeList (
				("Eye", EyeMakeup),
				("Lip", LipMakeup));

			return $"Makeup: {list}";
		}
	}
}