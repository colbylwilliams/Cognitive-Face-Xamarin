using System;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class IdentifyResultsTableViewController : FaceResultsTableViewController<IdentifyResultTableViewCell, IdentificationResult>
	{
		public IdentifyResultsTableViewController (IntPtr handle) : base (handle)
		{
		}
	}
}