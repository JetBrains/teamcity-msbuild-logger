namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;
    using Microsoft.Build.Framework;

    internal class ComparerContextNodeIdTargetId : IEqualityComparer<BuildEventContext>
    {
        public static readonly IEqualityComparer<BuildEventContext> Shared = new ComparerContextNodeIdTargetId();

        public bool Equals(BuildEventContext x, BuildEventContext y)
        {
            if (x == null || y == null || x.NodeId != y.NodeId || x.ProjectContextId != y.ProjectContextId)
            {
                return false;
            }

            return x.TargetId == y.TargetId;
        }

        public int GetHashCode(BuildEventContext x)
        {
            return x.ProjectContextId + (x.NodeId << 24);
        }
    }
}
