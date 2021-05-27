namespace TeamCity.MSBuild.Logger
{
    using JetBrains.Annotations;

    internal interface IHierarchicalMessageWriter
    {
        void StartBlock([NotNull] string name);

        void FinishBlock();
    }
}