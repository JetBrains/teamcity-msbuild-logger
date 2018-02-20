namespace TeamCity.MSBuild.Logger
{
    internal interface IHierarchicalMessageWriter
    {
        void StartBlock([NotNull] string name);

        void FinishBlock();
    }
}