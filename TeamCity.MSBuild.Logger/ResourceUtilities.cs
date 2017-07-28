namespace TeamCity.MSBuild.Logger
{
    internal static class ResourceUtilities
    {
        public static string FormatResourceString(string resourceName, params object[] args)
        {
            var formatString = Strings.ResourceManager.GetString(resourceName);
            return FormatString(formatString, args);
        }

        private static string FormatString(string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }
    }
}