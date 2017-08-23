namespace TeamCity.MSBuild.Logger
{
    internal interface IHierarchicalMessageWriter
    {
        void SelectFlow([CanBeNull] int? flowId);

        void StartBlock([NotNull] string name);

        void FinishBlock();
    }
}