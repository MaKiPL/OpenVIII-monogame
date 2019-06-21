using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;

namespace OpenVIII
{
    public sealed class ExceptionList : IDisposable
    {
        private readonly List<Exception> _exceptions = new List<Exception>();

        public void Add(Exception ex)
        {
            Debug.WriteLine(ex); // doesn't seem like this list is used correctly. so adding this writeline to atleast spit out the error.
            _exceptions.Add(ex);
        }

        public void Clear()
        {
            _exceptions.Clear();
        }

        public void Dispose()
        {
            switch (_exceptions.Count)
            {
                case 0:
                {
                    return;
                }
                case 1:
                {
                    ExceptionDispatchInfo.Capture(_exceptions[0]).Throw();
                    return;
                }
                default:
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var ex in _exceptions)
                        sb.AppendLine(ex.Message);

                    throw new AggregateException(sb.ToString(), _exceptions);
                }
            }
        }
    }
}
