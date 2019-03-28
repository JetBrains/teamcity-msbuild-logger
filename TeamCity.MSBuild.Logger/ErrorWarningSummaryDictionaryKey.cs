namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;

    internal class ErrorWarningSummaryDictionaryKey
    {
        internal ErrorWarningSummaryDictionaryKey(BuildEventContext entryPoint, string targetName)
        {
            EntryPointContext = entryPoint;
            TargetName = targetName ?? string.Empty;
        }

        public BuildEventContext EntryPointContext { get; }

        public string TargetName { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is ErrorWarningSummaryDictionaryKey summaryDictionaryKey) || !ComparerContextNodeId.Shared.Equals(EntryPointContext, summaryDictionaryKey.EntryPointContext))
            {
                return false;
            }

            return string.Compare(TargetName, summaryDictionaryKey.TargetName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override int GetHashCode()
        {
            return EntryPointContext.GetHashCode() + TargetName.GetHashCode();
        }
    }
}