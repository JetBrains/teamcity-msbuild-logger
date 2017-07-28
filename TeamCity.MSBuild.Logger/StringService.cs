namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Globalization;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class StringService : IStringService
    {
        public string UnescapeAll(string escapedString)
        {
            return UnescapeAll(escapedString, out bool _);
        }

        public string FormatResourceString(string resourceName, params object[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            var formatString = Strings.ResourceManager.GetString(resourceName);
            return string.IsNullOrEmpty(formatString) ? string.Empty : FormatString(formatString, args);
        }

        [CanBeNull]
        private static string UnescapeAll([CanBeNull] string escapedString, out bool escapingWasNecessary)
        {
            escapingWasNecessary = false;
            if (string.IsNullOrEmpty(escapedString))
            {
                return escapedString;
            }

            var num = escapedString.IndexOf('%');
            if (num == -1)
            {
                return escapedString;
            }

            var sb = StringBuilderCache.Acquire(escapedString.Length);
            var startIndex = 0;
            for (; num != -1; num = escapedString.IndexOf('%', num + 1))
            {
                if (num <= escapedString.Length - 3 && IsHexDigit(escapedString[num + 1]) && IsHexDigit(escapedString[num + 2]))
                {
                    sb.Append(escapedString, startIndex, num - startIndex);
                    var ch = (char)int.Parse(escapedString.Substring(num + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    sb.Append(ch);
                    startIndex = num + 3;
                    escapingWasNecessary = true;
                }
            }

            sb.Append(escapedString, startIndex, escapedString.Length - startIndex);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static bool IsHexDigit(char character)
        {
            if (character >= 48 && character <= 57 || character >= 65 && character <= 70)
            {
                return true;
            }

            if (character >= 97)
            {
                return character <= 102;
            }

            return false;
        }

        private static string FormatString([NotNull] string formatString, [NotNull] params object[] args)
        {
            if (formatString == null) throw new ArgumentNullException(nameof(formatString));
            if (args == null) throw new ArgumentNullException(nameof(args));
            return string.Format(formatString, args);
        }
    }
}
