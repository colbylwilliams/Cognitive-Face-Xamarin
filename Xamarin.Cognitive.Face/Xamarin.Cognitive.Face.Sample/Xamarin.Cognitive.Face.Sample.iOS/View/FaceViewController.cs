using System;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class FaceViewController : BaseViewController
	{
		class Segues
		{
			public const string Verification = "Verification";
		}


		public FaceViewController (IntPtr handle) : base (handle)
		{
		}


		partial void VerificationAction (NSObject sender)
		{
			ChooseVerify ().Forget ();
		}


		async Task ChooseVerify ()
		{
			var verificationType = await this.ShowActionSheet ("Please Choose", "Choose verification type",
													 ("Face and Face", VerificationType.FaceAndFace),
													 ("Face and Person", VerificationType.FaceAndPerson));

			if (verificationType == VerificationType.None)
			{
				return;
			}

			FaceState.Current.Verification.Type = verificationType;

			PerformSegue (Segues.Verification, this);
		}
	}
}