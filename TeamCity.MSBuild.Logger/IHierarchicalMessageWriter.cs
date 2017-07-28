namespace TeamCity.MSBuild.Logger
{
    internal interface IHierarchicalMessageWriter
    {
        void StartBlock(HierarchicalKey key, [NotNull] string name, [CanBeNull] string message = "");

        void FinishBlock(HierarchicalKey key, [CanBeNull] string message = "");
    }
}