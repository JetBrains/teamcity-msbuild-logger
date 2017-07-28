namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;
    using Microsoft.Build.Framework;

    internal class ComparerContextNodeId : IEqualityComparer<BuildEventContext>
    {
        public static readonly IEqualityComparer<BuildEventContext> Shared = new ComparerContextNodeId();

        private ComparerContextNodeId()
        {
        }

        public bool Equals(BuildEventContext x, BuildEventContext y)
        {
            if (x == null || y == null || x.NodeId != y.NodeId)
            {
                return false;
            }

            return x.ProjectContextId == y.ProjectContextId;
        }

        public int GetHashCode(BuildEventContext x)
        {
            return x.ProjectContextId + (x.NodeId << 24);
        }
    }
}
