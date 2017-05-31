using System;

namespace Xamarin.Cognitive.Face.Sample.Shared
{
	public class TrainingStatus
	{
		public TrainingStatusType Status { get; set; }

		public DateTime? CreatedDateTime { get; set; }

		public DateTime? LastActionDateTime { get; set; }

		public string Message { get; set; }


		public enum TrainingStatusType
		{
			NotStarted,
			Succeeded,
			Failed,
			Running
		}


		public static TrainingStatus FromStatus (TrainingStatusType status)
		{
			return new TrainingStatus
			{
				Status = status
			};
		}
	}
}