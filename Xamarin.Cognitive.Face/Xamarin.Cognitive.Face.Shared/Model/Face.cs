namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Represents a Face detected or persisted/saved via the Face API.
	/// </summary>
	public class Face : FaceModel
	{
		internal const string ThumbnailPathTemplate = "face-{0}.jpg";


		/// <summary>
		/// Gets or sets a full path to the thumbnail image stored for this Face.
		/// </summary>
		/// <value>The thumbnail path.</value>
		/// <remarks>Note that the Face API does not support retrieving face images for stored faces.  Use this path to save/load face images in client apps.</remarks>
		public string ThumbnailPath { get; set; }


		/// <summary>
		/// Gets the name of the file that can be used for saving/loading this Face's thumbnail image.
		/// </summary>
		/// <value>The name of the file.</value>
		internal string FileName => string.Format (ThumbnailPathTemplate, Id);


		/// <summary>
		/// Gets or sets the face rectangle that defines the bounds of this Face.
		/// </summary>
		/// <value>The face rectangle.</value>
		public FaceRectangle FaceRectangle { get; set; }


		/// <summary>
		/// Gets or sets the face landmarks for this Face.
		/// </summary>
		/// <value>The landmarks.</value>
		public FaceLandmarks Landmarks { get; set; }


		/// <summary>
		/// Gets or sets the <see cref="FaceAttributes"/> for this Face.
		/// </summary>
		/// <value>The attributes.</value>
		public FaceAttributes Attributes { get; set; }


		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Xamarin.Cognitive.Face.Model.Face"/> has facial hair.
		/// </summary>
		/// <value><c>true</c> if has facial hair; otherwise, <c>false</c>.</value>
		public bool HasFacialHair => Attributes?.FacialHair?.Mustache + Attributes?.FacialHair?.Beard + Attributes?.FacialHair?.Sideburns > 0;


		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Xamarin.Cognitive.Face.Model.Face"/> has makeup.
		/// </summary>
		/// <value><c>true</c> if has makeup; otherwise, <c>false</c>.</value>
		public bool HasMakeup => (Attributes?.Makeup?.EyeMakeup ?? false) || (Attributes?.Makeup?.LipMakeup ?? false);


		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Xamarin.Cognitive.Face.Model.Face"/> is occluded.
		/// </summary>
		/// <value><c>true</c> if is occluded; otherwise, <c>false</c>.</value>
		public bool IsOccluded => (Attributes?.Occlusion?.EyeOccluded ?? false) ||
					(Attributes?.Occlusion?.ForeheadOccluded ?? false) ||
					(Attributes?.Occlusion?.MouthOccluded ?? false);
	}
}