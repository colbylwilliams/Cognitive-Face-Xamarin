using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Person group - a group for grouping and organizing people and their faces.
	/// </summary>
	public class PersonGroup : FaceModel
	{
		/// <summary>
		/// Gets or sets the people for this group.
		/// </summary>
		/// <value>The people.</value>
		public List<Person> People { get; set; }


		/// <summary>
		/// Gets a value indicating whether this <see cref="PersonGroup"/> has had its <see cref="People"/> list initialized.
		/// </summary>
		/// <value><c>true</c> if people loaded; otherwise, <c>false</c>.</value>
		public bool PeopleLoaded
		{
			get
			{
				return People != null;
			}
		}
	}
}