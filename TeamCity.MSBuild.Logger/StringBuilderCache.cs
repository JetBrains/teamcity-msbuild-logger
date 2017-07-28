namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Text;

    internal static class StringBuilderCache
    {
        [ThreadStatic] private static StringBuilder _cachedInstance;

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

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity > 360)
            {
                return;
            }

            _cachedInstance = sb;
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            var str = sb.ToString();
            Release(sb);
            return str;
        }
    }
}
