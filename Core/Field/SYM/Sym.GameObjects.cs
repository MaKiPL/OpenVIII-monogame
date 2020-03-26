using System;
using System.Collections.Generic;


namespace OpenVIII.Fields
{
    public static partial class Sym
    {
        public sealed class GameObjects
        {
            private readonly OrderedDictionary<String, GameObject> _byIndex = new OrderedDictionary<String, GameObject>();
            private readonly Dictionary<Int32, ScriptReference> _byLabel = new Dictionary<Int32, ScriptReference>();
            private Int32 _maxLabel = -1;

            public GameObjects()
            {
            }

            public GameObject GetObjectOrDefault(Int32 index, String defaultValue = "Undefined_{0:D2}")
            {
                if (_byIndex.TryGetByIndex(index, out var obj))
                    return obj;

                return new GameObject(String.Format(defaultValue, index));
            }

            public Boolean FindByLabel(Int32 label, out GameObject obj, out String scriptName)
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

            internal void AddObject(String objectName)
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

            internal void AddScript(String objectName, String scriptName)
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
                public Int32 ScriptIndex { get; }

                public ScriptReference(GameObject obj, Int32 index)
                {
                    Object = obj;
                    ScriptIndex = index;
                }
            }
        }
    }
}
