using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Face group is a group of Faces returned from a Grouping operation.
	/// </summary>
	public class FaceGroup
	{
		/// <summary>
		/// Gets or sets the title of this FaceGroup.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }


		/// <summary>
		/// Gets or sets the face Ids that were grouped together.
		/// </summary>
		/// <value>The face identifiers.</value>
		public List<string> FaceIds { get; set; }


		/// <summary>
		/// Gets or sets the faces that were grouped together.
		/// </summary>
		/// <value>The faces.</value>
		public List<Face> Faces { get; set; }
	}
}