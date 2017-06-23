namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Facial hair: Consists of lengths of three facial hair areas: moustache, beard and sideburns.
	/// </summary>
	public class FacialHair : Attribute
	{
		/// <summary>
		/// Gets or sets the mustache value.
		/// </summary>
		/// <value>The mustache.</value>
		public float Mustache { get; set; }


		/// <summary>
		/// Gets or sets the beard value.
		/// </summary>
		/// <value>The beard.</value>
		public float Beard { get; set; }


		/// <summary>
		/// Gets or sets the sideburns value.
		/// </summary>
		/// <value>The sideburns.</value>
		public float Sideburns { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FacialHair"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FacialHair"/>.</returns>
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