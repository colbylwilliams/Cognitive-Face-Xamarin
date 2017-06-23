using System;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Training status indicates the training status of a <see cref="PersonGroup"/>.
	/// </summary>
	public class TrainingStatus
	{
		/// <summary>
		/// Gets or sets the Training status.
		/// </summary>
		/// <value>The training status.</value>
		public TrainingStatusType Status { get; set; }


		/// <summary>
		/// Gets or sets the created date time.  This is a combined UTC date and time string that describes person group created time.
		/// </summary>
		/// <value>The created date time.</value>
		public DateTime? CreatedDateTime { get; set; }


		/// <summary>
		/// Gets or sets the last action date time.  This is the person group last modify time in the UTC, could be null value when the person group is not successfully trained.
		/// </summary>
		/// <value>The last action date time.</value>
		public DateTime? LastActionDateTime { get; set; }


		/// <summary>
		/// Gets or sets the message: contains a failure message when training failed (omitted when training succeed).
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }


		/// <summary>
		/// Training status type.
		/// </summary>
		public enum TrainingStatusType
		{
			/// <summary>
			/// Training process is waiting to perform, or the group is new and a training process has not been initiated.
			/// </summary>
			NotStarted,
			/// <summary>
			/// Training has been completed.  This person group is ready for Face - Identify.
			/// </summary>
			Succeeded,
			/// <summary>
			/// Status failed is often caused by no persons or no persisted faces existing in the person group.
			/// </summary>
			Failed,
			/// <summary>
			/// The training process is ongoing.
			/// </summary>
			Running
		}


		/// <summary>
		/// Creates a <see cref="TrainingStatus"/> from a <see cref="TrainingStatusType"/>
		/// </summary>
		/// <returns>The <see cref="TrainingStatus"/>.</returns>
		/// <param name="status">The status to use.</param>
		public static TrainingStatus FromStatus (TrainingStatusType status)
		{
			return new TrainingStatus
			{
				Status = status
			};
		}


		/// <summary>
		/// Creates a <see cref="TrainingStatus"/> object from a string representation: notstarted, running, succeeded, failed.
		/// </summary>
		/// <returns>The <see cref="TrainingStatus"/></returns>
		/// <param name="status">Status string.</param>
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