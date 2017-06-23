namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Exposure level of the face.
	/// </summary>
	public class Exposure : Attribute
	{
		/// <summary>
		/// Gets or sets the exposure level of the face.
		/// </summary>
		/// <value>The exposure level.</value>
		public ExposureLevel ExposureLevel { get; set; }


		/// <summary>
		/// Gets or sets the exposure value.
		/// </summary>
		/// <value>The value.</value>
		public float Value { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Exposure"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Exposure"/>.</returns>
		public override string ToString ()
		{
			return $"Exposure Level: {ExposureLevel} ({Value})";
		}
	}
}