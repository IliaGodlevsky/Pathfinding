﻿using System;
using System.Linq;
using System.Reflection;

namespace Common.Extensions
{
    public static class MemberInfoExtensions
    {
        public static TAttribute GetAttributeOrNull<TAttribute>(this MemberInfo self, bool inherit = false)
            where TAttribute : Attribute
        {
            return Attribute.GetCustomAttribute(self, typeof(TAttribute), inherit) as TAttribute;
        }

        public static bool TryCreateDelegate<TDelegate>(this MethodInfo self, object target,
            out TDelegate action) where TDelegate : Delegate
        {
            try
            {
                action = (TDelegate)self.CreateDelegate(typeof(TDelegate), target);
                return true;
            }
            catch
            {
                action = null;
                return false;
            }
        }

        public static bool Implements<TInterface>(this Type type)
            where TInterface : class
        {
            return type.GetInterface(typeof(TInterface).Name) != null;
        }
    }
}