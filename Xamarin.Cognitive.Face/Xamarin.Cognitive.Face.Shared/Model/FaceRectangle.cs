namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Face rectangle defines the boundaries for a <see cref="Face"/> within the source image.
	/// </summary>
	public class FaceRectangle
	{
		/// <summary>
		/// Gets or sets the left edge of the face.
		/// </summary>
		/// <value>The left.</value>
		public float Left { get; set; }


		/// <summary>
		/// Gets or sets the top edge of the face.
		/// </summary>
		/// <value>The top.</value>
		public float Top { get; set; }


		/// <summary>
		/// Gets or sets the width of the face within the image.
		/// </summary>
		/// <value>The width.</value>
		public float Width { get; set; }


		/// <summary>
		/// Gets or sets the height of the face within the image.
		/// </summary>
		/// <value>The height.</value>
		public float Height { get; set; }
	}
}