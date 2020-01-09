using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static partial class Sym
    {
        public sealed class GameObject
        {
            public String Name { get; }
            
            private readonly List<String> _names = new List<String>();

            internal GameObject(String objectName)
            {
                if (String.IsNullOrWhiteSpace(objectName))
                    throw new ArgumentException($"Object name cannot be empty.", nameof(objectName));

                Name = objectName;
            }

            public String GetScriptName(Int32 index, String defaultValue = "Undefined_{0:D2}")
            {
                if (index < _names.Count)
                    return _names[index];

                return String.Format(defaultValue, index);
            }

            internal Int32 AddScript(String scriptName)
            {
                if (String.IsNullOrWhiteSpace(scriptName))
                    throw new ArgumentException($"Script name cannot be empty.", nameof(scriptName));

                _names.Add(scriptName);
                return _names.Count - 1;
            }
        }
    }
}
