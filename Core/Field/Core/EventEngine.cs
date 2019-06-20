using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


namespace OpenVIII
{
    public sealed class EventEngine
    {
        private readonly OrderedDictionary<Int32, FieldObject> _objects = new OrderedDictionary<Int32, FieldObject>();

        public FieldObject CurrentObject { get; private set; }

        public EventEngine()
        {
        }

        public void RegisterObject(FieldObject obj)
        {
            _objects.Add(obj.Id, obj);
        }

        public FieldObject GetObject(Int32 objectIndex)
        {
            if (_objects.TryGetByIndex(objectIndex, out var obj))
                return obj;

            throw new ArgumentException($"An object (Id: {objectIndex}) isn't registered in the event engine.", nameof(objectIndex));
        }

        public void Reset()
        {
            _objects.Clear();
        }

        public void Update(IServices services)
        {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (var obj in _objects.Values)
            {
                Console.WriteLine("Object: {0}", obj.InternalName);
                CurrentObject = obj;
                obj.Scripts.Update(services);
            }

            CurrentObject = null;

            Console.WriteLine("============ Milliseconds: {0} ============", sw.Elapsed.Milliseconds);
        }
    }
}