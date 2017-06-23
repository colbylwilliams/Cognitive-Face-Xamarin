namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Verification type contains the types of verification that can be performed.
	/// </summary>
	public enum VerificationType
	{
		/// <summary>
		/// None.  This indicates no verification should happen.
		/// </summary>
		None,

		/// <summary>
		/// Verify whether two faces belong to the same person.
		/// </summary>
		FaceAndFace,

		/// <summary>
		/// Verify whether one face belongs to a person.
		/// </summary>
		FaceAndPerson
	}
}