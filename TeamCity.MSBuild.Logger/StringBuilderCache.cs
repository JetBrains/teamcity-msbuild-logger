﻿namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    internal static class StringBuilderCache
    {
        [ThreadStatic] private static StringBuilder _cachedInstance;

        [SuppressMessage("ReSharper", "InvertIf")]
        public static StringBuilder Acquire(int capacity = 16)
        {
            if (capacity <= 360)
            {
                var tCachedInstance = _cachedInstance;
                if (tCachedInstance != null && capacity <= tCachedInstance.Capacity)
                {
                    _cachedInstance = null;
                    tCachedInstance.Length = 0;
                    return tCachedInstance;
                }
            }

            return new StringBuilder(capacity);
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            var str = sb.ToString();
            Release(sb);
            return str;
        }

        private static void Release(StringBuilder sb)
        {
            if (sb.Capacity > 360)
            {
                return;
            }

            _cachedInstance = sb;
        }
    }
}
