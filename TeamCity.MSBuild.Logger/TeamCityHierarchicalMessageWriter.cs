namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.Annotations;
    using JetBrains.TeamCity.ServiceMessages;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityHierarchicalMessageWriter : IHierarchicalMessageWriter, ILogWriter, IDisposable
    {
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly IColorStorage _colorStorage;
        [NotNull] private readonly Dictionary<int, Flow> _flows = new Dictionary<int, Flow>();
        [NotNull] private readonly IColorTheme _colorTheme;
        [NotNull] private readonly ITeamCityWriter _writer;
        [NotNull] private readonly IServiceMessageParser _serviceMessageParser;
        [NotNull] private readonly Dictionary<Flow, MessageInfo> _messages = new Dictionary<Flow, MessageInfo>();
        [NotNull] private readonly List<string> _buildProblems = new List<string>();

        private static int FlowId => HierarchicalContext.Current.FlowId;

        public TeamCityHierarchicalMessageWriter(
            [NotNull] ILoggerContext context,
            [NotNull] IColorTheme colorTheme,
            [NotNull] ITeamCityWriter writer,
            [NotNull] IServiceMessageParser serviceMessageParser,
            [NotNull] IColorStorage colorStorage)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _colorStorage = colorStorage ?? throw new ArgumentNullException(nameof(colorStorage));
            _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _serviceMessageParser = serviceMessageParser ?? throw new ArgumentNullException(nameof(serviceMessageParser));
        }

        private bool TryGetFlow(int flowId, out Flow flow, bool forceCreate)
        {
            if (!_flows.TryGetValue(flowId, out flow))
            {
                if (forceCreate)
                {
                    flow = new Flow(_writer, flowId == HierarchicalContext.DefaultFlowId);
                    _flows.Add(FlowId, flow);
                    return true;
                }

                return false;
            }

            return true;
        }

        public void StartBlock(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (TryGetFlow(FlowId, out var flow, true))
            {
                flow.StartBlock(name.Trim());
            }
        }

        public void FinishBlock()
        {
            if (TryGetFlow(FlowId, out var flow, false))
            {
                Write("\n", flow);
                flow.FinishBlock();
                if (flow.IsFinished)
                {
                    _flows.Remove(FlowId);
                    flow.Dispose();
                }
            }
        }

        public void Write(string message, IConsole console = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (TryGetFlow(FlowId, out var flow, false) || TryGetFlow(HierarchicalContext.DefaultFlowId, out flow, true))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Write(message, flow);
            }
        }

        public void SetColor(Color color)
        {
            _colorStorage.SetColor(color);
        }

        public void ResetColor()
        {
            _colorStorage.ResetColor();
        }

        public void Dispose()
        {
            foreach (var flow in _flows.Values)
            {
                _colorStorage.ResetColor();
                Write("\n", flow);
                flow.Dispose();
            }

            _flows.Clear();
            if (_buildProblems.Count > 0)
            {
                _writer.WriteBuildProblem("msbuild", string.Join("\n", _buildProblems));
                _buildProblems.Clear();
            }
        }

        private void Write([NotNull] string message, [NotNull] Flow flow)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (flow == null) throw new ArgumentNullException(nameof(flow));

            if (!_messages.TryGetValue(flow, out var messageInfo))
            {
                messageInfo = new MessageInfo();
                _messages.Add(flow, messageInfo);
            }

            messageInfo.Text.Append(message);
            messageInfo.Color = _colorStorage.Color;

            if (!message.EndsWith("\n"))
            {
                return;
            }

            _messages.Remove(flow);
            var messageState = MessageState.Normal;
            if (messageInfo.Color.HasValue)
            {
                switch (messageInfo.Color.Value)
                {
                    case Color.Error:
                        messageState = MessageState.Error;
                        break;

                    case Color.ErrorSummary:
                        _buildProblems.Add(messageInfo.Text.ToString().TrimEnd());
                        return;

                    case Color.Warning:
                    case Color.WarningSummary:
                        messageState = MessageState.Warning;
                        break;
                }
            }

            var text = messageInfo.Text.ToString().TrimEnd();
            var hasServiceMessage = false;
            if (_context.Parameters.PlainServiceMessage)
            {

                // TeamCity service message
                var trimed = text.TrimStart();
                if (trimed.StartsWith("##teamcity[", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var serviceMessage in _serviceMessageParser.ParseServiceMessages(trimed))
                    {
                        hasServiceMessage = true;
                        flow.Write(serviceMessage);
                    }
                }
            }

            // MSBuild output
            if (!hasServiceMessage)
            {
                flow.Write(FormatMessage(messageState, messageInfo, text), messageState);
            }
        }

        private string FormatMessage(MessageState messageState, MessageInfo messageInfo, string text)
        {
            return messageState == MessageState.Normal && messageInfo.Color.HasValue ? $"\x001B[{_colorTheme.GetAnsiColor(messageInfo.Color.Value)}m{text}" : text;
        }

        private enum MessageState
        {
            Normal,

            Warning,

            Error
        }

        private class MessageInfo
        {
            public readonly StringBuilder Text = new StringBuilder();

            public Color? Color;
        }

        private class Flow: IDisposable
        {
            private ITeamCityWriter _writer;
            private readonly Stack<ITeamCityWriter> _blocks = new Stack<ITeamCityWriter>();
            private readonly bool _isMainFlow;

            public Flow([NotNull] ITeamCityWriter writer, bool isMainFlow)
            {
                if (writer == null) throw new ArgumentNullException(nameof(writer));
                _isMainFlow = isMainFlow;
                _writer = isMainFlow ? writer : writer.OpenFlow();
            }

            public bool IsFinished => !_isMainFlow && _blocks.Count == 0;

            public void StartBlock(string name)
            {
                var newWriter = _writer.OpenBlock(name);
                _blocks.Push(_writer);
                _writer = newWriter;
            }

            public void FinishBlock()
            {
                var prevWriter = _blocks.Pop();
                _writer.Dispose();
                _writer = prevWriter;
            }

            public void Write([NotNull] string message, MessageState messageState)
            {
                if (message == null) throw new ArgumentNullException(nameof(message));
                switch (messageState)
                {
                    case MessageState.Warning:
                        _writer.WriteWarning(message);
                        break;

                    case MessageState.Error:
                        _writer.WriteError(message);
                        break;

                    default:
                        _writer.WriteMessage(message);
                        break;
                }
            }

            public void Write([NotNull] IServiceMessage message)
            {
                if (message == null) throw new ArgumentNullException(nameof(message));
                _writer.WriteRawMessage(message);
            }

            public void Dispose()
            {
                if (!_isMainFlow)
                {
                    _writer.Dispose();
                }
            }
        }
    }
}
