namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Blur level of the face.
	/// </summary>
	public class Blur : Attribute
	{
		/// <summary>
		/// Gets or sets the Blur level of the face. The level include `Low`, `Medium` and `High`.
		/// </summary>
		/// <value>The blur level.</value>
		public BlurLevel BlurLevel { get; set; }


		/// <summary>
		/// Gets or sets the blur value.  Larger value means the face is more blurry.
		/// </summary>
		/// <value>The value.</value>
		public float Value { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Blur"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Blur"/>.</returns>
		public override string ToString ()
		{
			return $"Blur Level: {BlurLevel} ({Value})";
		}
	}
}