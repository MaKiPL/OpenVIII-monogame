using System;
using System.Reflection;

namespace OpenVIII.NetFramework
{
    public static class TypeExtensionMethods
    {
        public static Type RequireType(this Assembly assembly, string typeName)
        {
            return assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
        }

        public static FieldInfo RequireStaticField(this Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                   ?? throw new NullReferenceException($"Cannot find the static field [{fieldName}] of type [{type}].");
        }
    }
}