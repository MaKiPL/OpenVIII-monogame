using System;
using System.Reflection;

namespace FF8.NetFramework
{
    public static class TypeExtensionMethods
    {
        public static Type RequireType(this Assembly assembly, String typeName)
        {
            return assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
        }

        public static FieldInfo RequireStaticField(this Type type, String fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                   ?? throw new NullReferenceException($"Cannot find the static field [{fieldName}] of type [{type}].");
        }
    }
}