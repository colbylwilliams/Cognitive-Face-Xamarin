using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// A Person within a <see cref="PersonGroup"/> that can persist one or more <see cref="Face"/> and is used for Identification and Verification operations.
	/// </summary>
	public class Person : FaceModel
	{
		/// <summary>
		/// Gets or sets the persisted/saved face Ids for this Person.  These can be used to restore the <see cref="Faces"/> for this Person.
		/// </summary>
		/// <value>The face identifiers.</value>
		public List<string> FaceIds { get; set; } = new List<string> ();


		/// <summary>
		/// Gets or sets the faces loaded for this Person.
		/// </summary>
		/// <value>The faces.</value>
		public List<Face> Faces { get; set; } = new List<Face> ();
	}
}