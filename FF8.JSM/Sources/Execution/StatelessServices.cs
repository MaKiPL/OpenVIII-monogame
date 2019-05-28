using System;
using FF8.Core;

namespace FF8.JSM.Format
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