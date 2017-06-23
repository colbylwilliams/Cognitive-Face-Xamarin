namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// The match mode for a FindSimilar operation.
	/// </summary>
	public enum FindSimilarMatchMode
	{
		/// <summary>
		/// Match person mode.  This is the default mode, and tries to find faces of the same person as possible by using internal same-person thresholds.  
		/// It is useful to find a known person's other photos.  Note that an empty list will be returned if no faces pass the internal thresholds.
		/// </summary>
		MatchPerson,
		/// <summary>
		/// Match face mode.  This ignores same-person thresholds and returns ranked similar faces anyway, even the similarity is low.  
		/// It can be used in the cases like searching celebrity-looking faces.
		/// </summary>
		MatchFace
	}
}