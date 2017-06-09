using System.Drawing;

namespace Xamarin.Cognitive.Face.Shared
{
	public class Face : FaceModel
	{
		public const string PhotoPathTemplate = "face-{0}.jpg";


		public string PhotoPath { get; set; }


		public string FileName => string.Format (PhotoPathTemplate, Id);


		public RectangleF FaceRectangle { get; set; }


		public FaceLandmarks Landmarks { get; set; }


		public FaceAttributes Attributes { get; set; }


		public bool HasFacialHair => Attributes?.FacialHair?.Mustache + Attributes?.FacialHair?.Beard + Attributes?.FacialHair?.Sideburns > 0;


		public bool HasMakeup => (Attributes?.Makeup?.EyeMakeup ?? false) || (Attributes?.Makeup?.LipMakeup ?? false);


		public bool IsOccluded => (Attributes?.Occlusion?.EyeOccluded ?? false) ||
					(Attributes?.Occlusion?.ForeheadOccluded ?? false) ||
					(Attributes?.Occlusion?.MouthOccluded ?? false);
	}
}