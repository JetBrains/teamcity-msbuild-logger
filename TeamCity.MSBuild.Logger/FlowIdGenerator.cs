// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.MSBuild.Logger
{
    using System;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;

    internal class FlowIdGenerator: IFlowIdGenerator
    {
        private readonly Parameters _parameters;
        private bool _isFirst = true;

        public FlowIdGenerator(Parameters parameters) => 
            _parameters = parameters;

        public string NewFlowId()
        {
            if (_isFirst)
            {
                _isFirst = false;
                var flowId = _parameters.FlowId;
                if (!string.IsNullOrWhiteSpace(flowId))
                {
                    return flowId;
                }
            }
            
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}