namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TeamCityHierarchicalMessageWriter : IHierarchicalMessageWriter, ILogWriter, IDisposable
    {
        private const int DefaultFlowId = 0;
        [NotNull] private readonly Func<IMessageWriter> _messageWriter;
        [NotNull] private readonly Dictionary<int, Flow> _flows = new Dictionary<int, Flow>();
        [NotNull] private readonly IColorTheme _colorTheme;
        private readonly ITeamCityWriter _writer;
        private readonly Dictionary<Flow, MessageInfo> _messages = new Dictionary<Flow, MessageInfo>();
        private Color? _currentColor;
        private int _flowId = DefaultFlowId;

        public TeamCityHierarchicalMessageWriter(
            [NotNull] IColorTheme colorTheme,
            [NotNull] ITeamCityWriter writer,
            [NotNull] Func<IMessageWriter> messageWriter)
        {
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        private bool TryGetFlow(int flowId, out Flow flow, bool forceCreate)
        {
            if (!_flows.TryGetValue(flowId, out flow))
            {
                if (forceCreate)
                {
                    flow = new Flow(_writer, flowId == DefaultFlowId);
                    _flows.Add(_flowId, flow);
                    return true;
                }

                return false;
            }

            return true;
        }

        public void SelectFlow(int? flowId)
        {
            _flowId = flowId ?? DefaultFlowId;
        }

        public void StartBlock(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (TryGetFlow(_flowId, out Flow flow, true))
            {
                flow.StartBlock(_messageWriter().IndentString(name).TrimEnd());
            }
        }

        public void FinishBlock()
        {
            if (TryGetFlow(_flowId, out Flow flow, false))
            {
                Write("\n", flow);
                flow.FinishBlock();
                if (flow.IsFinished)
                {
                    _flows.Remove(_flowId);
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

            if (TryGetFlow(_flowId, out Flow flow, false) || TryGetFlow(DefaultFlowId, out flow, true))
            {
                Write(message, flow);
            }
        }

        public void SetColor(Color color)
        {
            _currentColor = color;
        }

        public void ResetColor()
        {
            _currentColor = null;
        }

        public void Dispose()
        {
            foreach (var flow in _flows.Values)
            {
                Write("\n", flow);
                flow.Dispose();
            }

            _flows.Clear();
        }

        private void Write([NotNull] string message, [NotNull] Flow flow)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (flow == null) throw new ArgumentNullException(nameof(flow));

            if (!_messages.TryGetValue(flow, out MessageInfo messageInfo))
            {
                messageInfo = new MessageInfo();
                _messages.Add(flow, messageInfo);
            }

            messageInfo.Text.Append(message);
            messageInfo.Color = _currentColor;

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

                    case Color.Warning:
                        messageState = MessageState.Warning;
                        break;
                }
            }

            var text = messageInfo.Text.ToString().TrimEnd();
            flow.Write(messageState == MessageState.Normal && messageInfo.Color.HasValue ? $"\x001B[{_colorTheme.GetAnsiColor(messageInfo.Color.Value)}m{text}" : text, messageState);
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
