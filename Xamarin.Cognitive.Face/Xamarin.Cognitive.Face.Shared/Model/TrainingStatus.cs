using System;

namespace Xamarin.Cognitive.Face.Model
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


		/// <summary>
		/// Creates a TrainingStatus object from a string representation: notstarted, running, succeeded, failed.
		/// </summary>
		/// <returns>TrainingStatus</returns>
		/// <param name="status">Status.</param>
		public static TrainingStatus FromString (string status)
		{
			if (Enum.TryParse (status, true, out TrainingStatusType type))
			{
				return FromStatus (type);
			}

			return null;
		}
	}
}