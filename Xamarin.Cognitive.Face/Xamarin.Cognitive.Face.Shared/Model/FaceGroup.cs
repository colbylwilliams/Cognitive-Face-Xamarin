using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	public class FaceGroup
	{
		public string Title { get; set; }

		public List<string> FaceIds { get; set; }

		public List<Face> Faces { get; set; }
	}
}