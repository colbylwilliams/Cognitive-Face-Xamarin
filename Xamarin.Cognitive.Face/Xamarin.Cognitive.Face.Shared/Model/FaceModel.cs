namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Base class for Face API model classes.
	/// </summary>
	public abstract class FaceModel
	{
		/// <summary>
		/// Gets or sets the identifier for this object.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }


		/// <summary>
		/// Gets or sets the name of this object.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the user data for this object.
		/// </summary>
		/// <value>The user data.</value>
		public string UserData { get; set; }
	}
}