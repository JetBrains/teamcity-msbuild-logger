namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultHierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        public void SelectFlow(int? flowId)
        {
        }

        public void StartBlock(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
        }

        public void FinishBlock()
        {
        }
    }
}
