namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Occlusion: Whether face area (forehead, eye, mouth) is occluded or not.
	/// </summary>
	public class Occlusion : Attribute
	{
		/// <summary>
		/// Gets or sets a value indicating whether the face's forehead is occluded.
		/// </summary>
		/// <value><c>true</c> if forehead occluded; otherwise, <c>false</c>.</value>
		public bool ForeheadOccluded { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the face's eye is occluded.
		/// </summary>
		/// <value><c>true</c> if eye occluded; otherwise, <c>false</c>.</value>
		public bool EyeOccluded { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the face's mouth is occluded.
		/// </summary>
		/// <value><c>true</c> if mouth occluded; otherwise, <c>false</c>.</value>
		public bool MouthOccluded { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Occlusion"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.Occlusion"/>.</returns>
		public override string ToString ()
		{
			var list = BuildTypeList (
				("Eye", EyeOccluded),
				("Mouth", MouthOccluded),
				("Forehead", ForeheadOccluded));

			return $"Occlusion: {list}";
		}
	}
}