namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Noise: the noise level of the face.
	/// </summary>
	public class Noise : Attribute
	{
		/// <summary>
		/// Gets or sets the noise level.  The level include `Low`, `Medium` and `High`.
		/// </summary>
		/// <value>The noise level.</value>
		public NoiseLevel NoiseLevel { get; set; }


		/// <summary>
		/// Gets or sets the noise value.  Larger value means more noisy the face is.
		/// </summary>
		/// <value>The value.</value>
		public float Value { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Noise"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Noise"/>.</returns>
		public override string ToString ()
		{
			return $"Noise Level: {NoiseLevel} ({Value})";
		}
	}
}