namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public interface IHandleResults<TResult>
	{
		void SetResult (TResult result);
	}
}