using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	public class PersonGroup : FaceModel
	{
		public List<Person> People { get; set; }

		public bool PeopleLoaded
		{
			get
			{
				return People != null;
			}
		}
	}
}