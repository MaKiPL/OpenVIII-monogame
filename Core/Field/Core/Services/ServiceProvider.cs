using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public sealed class ServiceProvider : IServices
    {
        #region Fields

        private readonly Dictionary<object, object> _services = new Dictionary<object, object>();

        #endregion Fields

        #region Methods

        public void Register<T>(ServiceId<T> id, T service) => _services.Add(id, service);

        public T Service<T>(ServiceId<T> id)
        {
            if (_services.TryGetValue(id, out var value))
                return (T)value;

            throw new ArgumentException($"Service {typeof(T).FullName} isn't registered.", nameof(id));
        }

        #endregion Methods
    }
}