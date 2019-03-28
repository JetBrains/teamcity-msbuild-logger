namespace TeamCity.MSBuild.Logger
{
    using System;
    using IoC;

    internal class HierarchicalContext: IDisposable
    {
        public static readonly int DefaultFlowId = 0;
        private static readonly HierarchicalContext Default = new HierarchicalContext(0);

        [CanBeNull][ThreadStatic] private static HierarchicalContext _currentHierarchicalContext;
        private readonly HierarchicalContext _prevHierarchicalContext;

        public HierarchicalContext([CanBeNull] int? flowId)
        {
            FlowId = flowId ?? DefaultFlowId;
            _prevHierarchicalContext = _currentHierarchicalContext;
            _currentHierarchicalContext = this;
        }

        [NotNull]
        public static HierarchicalContext Current => _currentHierarchicalContext ?? Default;

        public int FlowId { get; }

        public void Dispose()
        {
            _currentHierarchicalContext = _prevHierarchicalContext;
        }
    }
}
