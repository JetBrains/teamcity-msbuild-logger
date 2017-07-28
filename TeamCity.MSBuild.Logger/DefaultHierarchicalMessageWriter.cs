namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultHierarchicalMessageWriter : IHierarchicalMessageWriter
    {
        [NotNull] private readonly IMessageWriter _messageWriter;

        public DefaultHierarchicalMessageWriter(
            [NotNull] IMessageWriter messageWriter)
        {
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        }

        public void FinishBlock(HierarchicalKey key, string message = "")
        {
            _messageWriter.WriteMessageAligned(message, true);
        }

        public void StartBlock(HierarchicalKey key, string name, string message = "")
        {
            _messageWriter.WriteMessageAligned(message, true);
        }
    }
}
