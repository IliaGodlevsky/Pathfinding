﻿using System;

namespace Shared.Singleton.Exceptions
{
    [Serializable]
    public class SingletonException : Exception
    {
        private static string GetMessage(Type genericType)
        {
            return string.Format("{0} has neither private nor protected parametreless constructor", genericType.Name);
        }

        internal SingletonException(Type type) : base(GetMessage(type))
        {

        }
    }
}