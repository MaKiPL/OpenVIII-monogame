using System;
using System.Diagnostics;

namespace OpenVIII.Fields
{
    public sealed class EventEngine
    {
        #region Fields

        private readonly OrderedDictionary<int, FieldObject> _objects = new OrderedDictionary<int, FieldObject>();

        #endregion Fields

        #region Constructors

        public EventEngine()
        {
        }

        #endregion Constructors

        #region Properties

        public FieldObject CurrentObject { get; set; }

        #endregion Properties

        #region Methods

        public FieldObject GetObject(int objectIndex)
        {
            if (_objects.TryGetByIndex(objectIndex, out var obj))
                return obj;

            throw new ArgumentException($"An object (Id: {objectIndex}) isn't registered in the event engine.", nameof(objectIndex));
        }

        public void RegisterObject(FieldObject obj) => _objects.Add(obj.Id, obj);

        public void Reset() => _objects.Clear();

        public void Update(IServices services)
        {
            var sw = Stopwatch.StartNew();

            foreach (var obj in _objects.Values)
            {
                Console.WriteLine("Object: {0}", obj.InternalName);
                CurrentObject = obj;
                obj.Scripts.Update(services);
            }

            CurrentObject = null;

            Console.WriteLine("============ Milliseconds: {0} ============", sw.Elapsed.Milliseconds);
        }

        #endregion Methods
    }
}