using System;
using System.Collections.Generic;

namespace FF8
{
    public sealed class ServiceProvider : IServices
    {
        private readonly Dictionary<Object, Object> _services = new Dictionary<Object, Object>();

        public void Register<T>(ServiceId<T> id, T service)
        {
            _services.Add(id, service);
        }

        public T Service<T>(ServiceId<T> id)
        {
            if (_services.TryGetValue(id, out var value))
                return (T)value;

            throw new ArgumentException($"Service {typeof(T).FullName} isn't registered.", nameof(id));
        }
    }
}