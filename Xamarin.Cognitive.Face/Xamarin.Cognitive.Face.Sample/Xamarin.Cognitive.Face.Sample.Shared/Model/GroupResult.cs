using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Shared
{
	public class GroupResult
	{
		public List<FaceGroup> Groups { get; set; }

		public FaceGroup MessyGroup { get; set; }

		public bool HasGroups => Groups?.Count > 0 || MessyGroup != null;
	}
}