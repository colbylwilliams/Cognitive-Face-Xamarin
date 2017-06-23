using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Model
{
	/// <summary>
	/// Base class for Attributes.
	/// </summary>
	public abstract class Attribute
	{
		/// <summary>
		/// Builds the string list of all input types, delimited by '|'.
		/// </summary>
		/// <returns>The type list.</returns>
		/// <param name="inputTypes">Input types.</param>
		public string BuildTypeList (params (string Name, bool Value) [] inputTypes)
		{
			var types = new List<string> ();

			foreach (var type in inputTypes)
			{
				if (type.Value)
				{
					types.Add (type.Name);
				}
			}

			if (types.Count > 0)
			{
				return string.Join ("|", types);
			}

			return "None";
		}


		/// <summary>
		/// Builds the string list of all input types, delimited by '|'.
		/// </summary>
		/// <returns>The type list.</returns>
		/// <param name="inputTypes">Input types.</param>
		public string BuildTypeList (params (string Name, float Value) [] inputTypes)
		{
			var types = new List<string> ();

			foreach (var type in inputTypes)
			{
				if (type.Value > 0)
				{
					types.Add ($"{type.Name} ({type.Value})");
				}
			}

			if (types.Count > 0)
			{
				return string.Join ("|", types);
			}

			return "None";
		}
	}
}