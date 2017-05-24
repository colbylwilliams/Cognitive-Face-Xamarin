using System.Drawing;

namespace Xamarin.Cognitive.Face.Sample.Shared
{
	public class Face : FaceModel
	{
		public const string PhotoPathTemplate = "face-{0}.jpg";


		public string PhotoPath { get; set; }


		public string FileName
		{
			get
			{
				return string.Format (PhotoPathTemplate, Id);
			}
		}


		public RectangleF FaceRectangle { get; set; }


		public FaceAttributes Attributes { get; set; }


		public bool HasFacialHair
		{
			get
			{
				return Attributes?.FacialHair?.Mustache + Attributes?.FacialHair?.Beard + Attributes?.FacialHair?.Sideburns > 0;
			}
		}


		public bool HasMakeup
		{
			get
			{
				return (Attributes?.Makeup?.EyeMakeup ?? false) || (Attributes?.Makeup?.LipMakeup ?? false);
			}
		}


		public bool IsOccluded
		{
			get
			{
				return (Attributes?.Occlusion?.EyeOccluded ?? false) ||
					(Attributes?.Occlusion?.ForeheadOccluded ?? false) ||
					(Attributes?.Occlusion?.MouthOccluded ?? false);
			}
		}
	}
}