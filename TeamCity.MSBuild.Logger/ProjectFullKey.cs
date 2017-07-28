namespace TeamCity.MSBuild.Logger
{
    using System.Globalization;
    using Microsoft.Build.Framework;

    internal class ProjectFullKey
    {
        public ProjectFullKey(int projectKey, int entryPointKey)
        {
            ProjectKey = projectKey;
            EntryPointKey = entryPointKey;
        }

        public int ProjectKey { get; }

        public int EntryPointKey { get; }

        public string ToString(LoggerVerbosity verbosity)
        {
            return verbosity <= LoggerVerbosity.Normal ? string.Format(CultureInfo.InvariantCulture, "{0}", ProjectKey) : ToString();
        }

        public override string ToString()
        {
            return EntryPointKey <= 1 ? string.Format(CultureInfo.InvariantCulture, "{0}", ProjectKey) : string.Format(CultureInfo.InvariantCulture, "{0}:{1}", ProjectKey, EntryPointKey);
        }

        public override bool Equals(object obj)
        {
            if (obj is ProjectFullKey projectFullKey && projectFullKey.ProjectKey == ProjectKey)
            {
                return projectFullKey.EntryPointKey == EntryPointKey;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ProjectKey + (EntryPointKey << 16);
        }
    }
}
