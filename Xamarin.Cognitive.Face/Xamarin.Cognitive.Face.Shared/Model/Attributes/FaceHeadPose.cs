namespace Xamarin.Cognitive.Face.Model
{
	public class FaceHeadPose : Attribute
	{
		public float Roll { get; set; }

		public float Yaw { get; set; }

		//HeadPose's pitch value is a reserved field and will always return 0.
		//public float Pitch { get; set; }

		public override string ToString ()
		{
			return $"Head Pose: {Roll}° roll, {Yaw}° yaw";
		}
	}
}