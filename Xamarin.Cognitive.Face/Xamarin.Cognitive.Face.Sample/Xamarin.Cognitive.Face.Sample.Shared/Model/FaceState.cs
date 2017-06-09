namespace Xamarin.Cognitive.Face.Shared
{
	public class FaceState
	{
		static FaceState state;
		public static FaceState Current => state ?? (state = new FaceState ());

		public PersonGroup CurrentGroup { get; set; }

		public Person CurrentPerson { get; set; }

		public bool NeedsTraining { get; set; }

		public VerificationDetails Verification { get; set; }


		public class VerificationDetails
		{
			public VerificationType Type { get; set; }

			public Shared.Face Face { get; internal set; }

			public Person Person { get; set; }

			public PersonGroup Group { get; set; }

			public bool NeedsPerson { get; set; }
		}


		FaceState ()
		{
			Verification = new VerificationDetails ();
		}
	}
}