namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Face head pose: 3-D roll/yew/pitch angles for face direction.
	/// </summary>
	public class FaceHeadPose : Attribute
	{
		/// <summary>
		/// Gets or sets the roll value.
		/// </summary>
		/// <value>The roll.</value>
		public float Roll { get; set; }


		/// <summary>
		/// Gets or sets the yaw value.
		/// </summary>
		/// <value>The yaw.</value>
		public float Yaw { get; set; }

		//HeadPose's pitch value is a reserved field and will always return 0.
		//public float Pitch { get; set; }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FaceHeadPose"/>.
		/// </summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Xamarin.Cognitive.Face.Model.FaceHeadPose"/>.</returns>
		public override string ToString ()
		{
			return $"Head Pose: {Roll}° roll, {Yaw}° yaw";
		}
	}
}