using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Group result is the result returned from a Grouping operation.
	/// </summary>
	public class GroupResult
	{
		/// <summary>
		/// Gets or sets the groups that were found in the set of Faces.
		/// </summary>
		/// <value>The groups.</value>
		public List<FaceGroup> Groups { get; set; }


		/// <summary>
		/// Gets or sets the messy group.  This will contain any faces not matched/grouped to other faces, if any.
		/// </summary>
		/// <value>The messy group.</value>
		/// <remarks>In this case all faces match into a group, this will be null.</remarks>
		public FaceGroup MessyGroup { get; set; }


		/// <summary>
		/// Gets a value indicating whether this GroupResult has any <see cref="FaceGroup"/>.
		/// </summary>
		/// <value><c>true</c> if has groups; otherwise, <c>false</c>.</value>
		public bool HasGroups => Groups?.Count > 0 || MessyGroup != null;
	}
}