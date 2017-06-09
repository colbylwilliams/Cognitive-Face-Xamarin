using System.Collections.Generic;

namespace Xamarin.Cognitive.Face.Shared
{
    public abstract class Attribute
    {
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