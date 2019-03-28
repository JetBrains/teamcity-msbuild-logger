namespace TeamCity.MSBuild.Logger
{
    using IoC;

    internal interface IHierarchicalMessageWriter
    {
        void StartBlock([NotNull] string name);

        void FinishBlock();
    }
}