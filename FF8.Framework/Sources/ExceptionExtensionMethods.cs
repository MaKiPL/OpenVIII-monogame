using System;
using System.Runtime.ExceptionServices;

namespace FF8.Framework
{
    public static class ExceptionExtensionMethods
    {
        public static T Rethrow<T>(this T exception) where T : Exception
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            throw exception;
        }
    }
}