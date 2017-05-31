using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Cognitive.Face.Sample.Shared.Utilities
{
	public class TaskQueue
	{
		readonly SemaphoreSlim semaphore;


		public TaskQueue (int concurrency = 1)
		{
			semaphore = new SemaphoreSlim (concurrency);
		}


		public async Task<T> Enqueue<T> (Func<Task<T>> taskGenerator)
		{
			await semaphore.WaitAsync ();

			try
			{
				return await taskGenerator ();
			}
			finally
			{
				semaphore.Release ();
			}
		}


		public async Task Enqueue (Func<Task> taskGenerator)
		{
			await semaphore.WaitAsync ();

			try
			{
				await taskGenerator ();
			}
			finally
			{
				semaphore.Release ();
			}
		}
	}
}