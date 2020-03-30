using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static partial class Sym
    {
        public sealed class GameObject
        {
            public string Name { get; }
            
            private readonly List<string> _names = new List<string>();

            internal GameObject(string objectName)
            {
                if (string.IsNullOrWhiteSpace(objectName))
                    throw new ArgumentException($"Object name cannot be empty.", nameof(objectName));

                Name = objectName;
            }

            public string GetScriptName(int index, string defaultValue = "Undefined_{0:D2}")
            {
                if (index < _names.Count)
                    return _names[index];

                return string.Format(defaultValue, index);
            }

            internal int AddScript(string scriptName)
            {
                if (string.IsNullOrWhiteSpace(scriptName))
                    throw new ArgumentException($"Script name cannot be empty.", nameof(scriptName));

                _names.Add(scriptName);
                return _names.Count - 1;
            }
        }
    }
}
