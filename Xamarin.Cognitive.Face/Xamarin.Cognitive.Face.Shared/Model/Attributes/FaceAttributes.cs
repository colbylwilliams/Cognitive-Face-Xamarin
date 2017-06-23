namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Represents all facial attributes of a given Face.
	/// </summary>
	public class FaceAttributes
	{
		/// <summary>
		/// Gets or sets the age.  An age number in years.
		/// </summary>
		/// <value>The age.</value>
		public float Age { get; set; }


		/// <summary>
		/// Gets or sets the smile intensity.  Smile intensity, a number between [0,1].
		/// </summary>
		/// <value>The smile intensity.</value>
		public float SmileIntensity { get; set; }


		/// <summary>
		/// Gets or sets the gender.  Male or female.
		/// </summary>
		/// <value>The gender.</value>
		public string Gender { get; set; }


		/// <summary>
		/// Gets or sets the glasses.  Glasses type: Possible values are 'NoGlasses', 'ReadingGlasses', 'Sunglasses', 'SwimmingGoggles'.
		/// </summary>
		/// <value>The glasses.</value>
		public Glasses? Glasses { get; set; }


		/// <summary>
		/// Gets or sets the facial hair.  Consists of lengths of three facial hair areas: moustache, beard and sideburns.
		/// </summary>
		/// <value>The facial hair.</value>
		public FacialHair FacialHair { get; set; }


		/// <summary>
		/// Gets or sets the head pose.  3-D roll/yew/pitch angles for face direction.  Pitch value is a reserved field and will always return 0.
		/// </summary>
		/// <value>The head pose.</value>
		public FaceHeadPose HeadPose { get; set; }


		/// <summary>
		/// Gets or sets the emotion.  Emotions intensity expressed by the face, including anger, contempt, disgust, fear, happiness, neutral, sadness and surprise.
		/// </summary>
		/// <value>The emotion.</value>
		public FaceEmotion Emotion { get; set; }


		/// <summary>
		/// Gets or sets the hair.  Return face features indicating whether the hair is visible, bald or not also including hair color if available.
		/// </summary>
		/// <value>The hair.</value>
		public Hair Hair { get; set; }


		/// <summary>
		/// Gets or sets the makeup.  Whether face area (eye, lip) is made-up or not.
		/// </summary>
		/// <value>The makeup.</value>
		public Makeup Makeup { get; set; }


		/// <summary>
		/// Gets or sets the occlusion.  Whether face area (forehead, eye, mouth) is occluded or not.
		/// </summary>
		/// <value>The occlusion.</value>
		public Occlusion Occlusion { get; set; }


		/// <summary>
		/// Gets or sets the accessories.  Accessory types on face or around face area, including headwear, glasses and mask.
		/// </summary>
		/// <value>The accessories.</value>
		public Accessories Accessories { get; set; }


		/// <summary>
		/// Gets or sets the blur.  Blur level of the face. The level include `Low`, `Medium` and `High`. Larger value means more blury the face is.
		/// </summary>
		/// <value>The blur.</value>
		public Blur Blur { get; set; }


		/// <summary>
		/// Gets or sets the exposure.  Exposure level of the face. The level include `GoodExposure`, `OverExposure` and `UnderExposure`.
		/// </summary>
		/// <value>The exposure.</value>
		public Exposure Exposure { get; set; }


		/// <summary>
		/// Gets or sets the noise.  Noise level of the face. The level include `Low`, `Medium` and `High`. Larger value means more noisy the face is.
		/// </summary>
		/// <value>The noise.</value>
		public Noise Noise { get; set; }
	}
}