namespace TeamCity.MSBuild.Logger
{
    internal interface IHierarchicalMessageWriter
    {
        void StartBlock(HierarchicalKey key, [NotNull] string name);

        void FinishBlock(HierarchicalKey key);
    }
}