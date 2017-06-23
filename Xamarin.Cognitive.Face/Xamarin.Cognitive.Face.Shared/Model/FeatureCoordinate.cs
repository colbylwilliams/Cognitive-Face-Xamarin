namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Feature coordinate indicates a point within a <see cref="FaceRectangle"/> that defines a facial feature.
	/// </summary>
	public class FeatureCoordinate
	{
		/// <summary>
		/// Gets or sets the x coordinate of the feature.
		/// </summary>
		/// <value>The x.</value>
		public float X { get; set; }


		/// <summary>
		/// Gets or sets the y coordinate of the feature.
		/// </summary>
		/// <value>The y.</value>
		public float Y { get; set; }
	}
}