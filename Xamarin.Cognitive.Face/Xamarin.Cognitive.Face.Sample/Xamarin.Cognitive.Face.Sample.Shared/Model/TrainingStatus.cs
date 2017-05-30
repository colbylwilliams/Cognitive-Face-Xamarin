using System;

namespace Xamarin.Cognitive.Face.Sample.Shared
{
	public class TrainingStatus
	{
		public string Status { get; set; }

		public DateTime? CreatedDateTime { get; set; }

		public DateTime? LastActionDateTime { get; set; }

		public string Message { get; set; }
	}
}