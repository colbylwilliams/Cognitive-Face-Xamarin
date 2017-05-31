using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Sample.Shared
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