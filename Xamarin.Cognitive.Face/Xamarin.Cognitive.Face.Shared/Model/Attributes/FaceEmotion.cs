namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Emotion intensity expressed by the face, including anger, contempt, disgust, fear, happiness, neutral, sadness and surprise.
	/// </summary>
	public class FaceEmotion : Attribute
	{
		/// <summary>
		/// Gets or sets the anger value.
		/// </summary>
		/// <value>The anger.</value>
		public float Anger { get; set; }


		/// <summary>
		/// Gets or sets the contempt value.
		/// </summary>
		/// <value>The contempt.</value>
		public float Contempt { get; set; }


		/// <summary>
		/// Gets or sets the disgust value.
		/// </summary>
		/// <value>The disgust.</value>
		public float Disgust { get; set; }


		/// <summary>
		/// Gets or sets the fear value.
		/// </summary>
		/// <value>The fear.</value>
		public float Fear { get; set; }


		/// <summary>
		/// Gets or sets the happiness value.
		/// </summary>
		/// <value>The happiness.</value>
		public float Happiness { get; set; }


		/// <summary>
		/// Gets or sets the neutral value.
		/// </summary>
		/// <value>The neutral.</value>
		public float Neutral { get; set; }


		/// <summary>
		/// Gets or sets the sadness value.
		/// </summary>
		/// <value>The sadness.</value>
		public float Sadness { get; set; }


		/// <summary>
		/// Gets or sets the surprise value.
		/// </summary>
		/// <value>The surprise.</value>
		public float Surprise { get; set; }


		/// <summary>
		/// Gets or sets the most emotion value.
		/// </summary>
		/// <value>The most emotion value.</value>
		public float MostEmotionValue { get; set; }


		/// <summary>
		/// Gets or sets the most emotion name.
		/// </summary>
		/// <value>The most emotion.</value>
		public string MostEmotion { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FaceEmotion"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FaceEmotion"/>.</returns>
		public override string ToString ()
		{
			return $"Emotion: {MostEmotion} ({MostEmotionValue})";
		}
	}
}