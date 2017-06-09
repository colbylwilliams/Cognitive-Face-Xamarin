using System;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Cognitive.Face.Shared.Extensions
{
	public static class TaskExtensions
	{
		/* TaskStatus enum
         * Created, = 0
         * WaitingForActivation, = 1
         * WaitingToRun, = 2
         * Running, = 3
         * WaitingForChildrenToComplete, = 4
         * RanToCompletion, = 5
         * Canceled, = 6
         * Faulted = 7  */
		public static bool IsNullFinishCanceledOrFaulted<T> (this TaskCompletionSource<T> tcs)
		{
			return tcs == null || (int) tcs.Task.Status >= 5;
		}


		//http://stackoverflow.com/a/22864616/812415
		public static async void Forget (this Task task, params Type [] acceptableExceptions)
		{
			try
			{
				await task.ConfigureAwait (false);
			}
			catch (Exception ex)
			{
				Log.Error ($"Exception thrown on unmonitored Task: {ex.Message}");

				// TODO: consider whether derived types are also acceptable.
				if (!acceptableExceptions.Contains (ex.GetType ()))
					throw;
			}
		}
	}
}