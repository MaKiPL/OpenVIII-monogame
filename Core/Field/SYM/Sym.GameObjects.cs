using System;
using System.Collections.Generic;


namespace OpenVIII.Fields
{
    public static partial class Sym
    {
        public sealed class GameObjects
        {
            private readonly OrderedDictionary<string, GameObject> _byIndex = new OrderedDictionary<string, GameObject>();
            private readonly Dictionary<int, ScriptReference> _byLabel = new Dictionary<int, ScriptReference>();
            private int _maxLabel = -1;

            public GameObjects()
            {
            }

            public GameObject GetObjectOrDefault(int index, string defaultValue = "Undefined_{0:D2}")
            {
                return _byIndex.TryGetByIndex(index, out var obj)
                    ? obj
                    : new GameObject(string.Format(defaultValue, index));
            }

            public bool FindByLabel(int label, out GameObject obj, out string scriptName)
            {
                if (_byLabel.TryGetValue(label, out var reference))
                {
                    obj = reference.Object;
                    scriptName = obj.GetScriptName(reference.ScriptIndex);
                    return true;
                }

                obj = null;
                scriptName = null;
                return false;
            }

            internal void AddObject(string objectName)
            {
                if (_byIndex.TryGetByKey(objectName, out var obj))
                {
                    var index = obj.AddScript("Initialize");
                    var reference = new ScriptReference(obj, index);
                    _byLabel.Add(++_maxLabel, reference);
                }
                else
                {
                    obj = new GameObject(objectName);
                    _byIndex.Add(objectName, obj);
                }
            }

            internal void AddScript(string objectName, string scriptName)
            {
                if (_byIndex.TryGetByKey(objectName, out var obj))
                {
                    var index = obj.AddScript(scriptName);
                    var reference = new ScriptReference(obj, index);
                    _byLabel.Add(++_maxLabel, reference);
                }
                else
                {
                    throw new ArgumentException($"Unexpected object occurred: {objectName}", nameof(objectName));
                }
            }

            private sealed class ScriptReference
            {
                public GameObject Object { get; }
                public int ScriptIndex { get; }

                public ScriptReference(GameObject obj, int index)
                {
                    Object = obj;
                    ScriptIndex = index;
                }
            }
        }
    }
}
