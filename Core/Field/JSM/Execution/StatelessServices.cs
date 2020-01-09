using System;

namespace OpenVIII.Fields
{
    public sealed class StatelessServices : IServices
    {
        public static IServices Instance { get; } = new StatelessServices();

        private StatelessServices()
        {
        }

        public T Service<T>(ServiceId<T> id)
        {
            return (T)(Object)id;
        }
    }
}