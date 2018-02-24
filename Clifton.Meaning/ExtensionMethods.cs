using System;
using System.Collections.Generic;
using System.Linq;

namespace Clifton.Meaning
{
    public static class ExtensionMethods
    {
        public static bool HasBaseClass<T>(this Type t)
        {
            return t.BaseType == typeof(T);
        }

        public static bool HasInterface<T>(this Type t)
        {
            return t.GetInterfaces().Any(i => i == typeof(T));
        }

        public static string AsStringList(this IReadOnlyList<ContextPath> contextPath)
        {
            return String.Join(".", contextPath.Select(cp => "[" + cp.PathType.ToString() + "]" + cp.Type.Name));
        }
    }
}
