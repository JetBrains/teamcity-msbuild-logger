namespace TeamCity.MSBuild.Logger
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections.Generic;
    using System.Text;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MessageWriter: IMessageWriter
    {
        [NotNull] private readonly IStringService _stringService;
        [NotNull] private readonly IEventFormatter _eventFormatter;
        [NotNull] private readonly ILogFormatter _logFormatter;
        [NotNull] private readonly IBuildEventManager _buildEventManager;
        private static readonly string[] NewLines = { "\r\n", "\n" };
        [NotNull] private readonly ILoggerContext _context;
        [NotNull] private readonly ILogWriter _logWriter;
        private readonly object _lockObject = new object();

        public MessageWriter(
            [NotNull] ILoggerContext context,
            [NotNull] ILogWriter logWriter,
            [NotNull] IBuildEventManager buildEventManager,
            [NotNull] ILogFormatter logFormatter,
            [NotNull] IEventFormatter eventFormatter,
            [NotNull] IStringService stringService)
        {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
            _eventFormatter = eventFormatter ?? throw new ArgumentNullException(nameof(eventFormatter));
            _logFormatter = logFormatter ?? throw new ArgumentNullException(nameof(logFormatter));
            _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        public void WriteLinePrefix(BuildEventContext e, DateTime eventTimeStamp, bool isMessagePrefix)
        {
            WriteLinePrefix(_context.GetFullProjectKey(e).ToString(_context.Verbosity), eventTimeStamp, isMessagePrefix);
        }

        public void WriteMessageAligned(string message, bool prefixAlreadyWritten, int prefixAdjustment = 0)
        {
            lock (_lockObject)
            {
                var adjustedPrefixWidth = _context.PrefixWidth + prefixAdjustment;
                var strArray = SplitStringOnNewLines(message);
                for (var index = 0; index < strArray.Length; ++index)
                {
                    var nonNullMessage = strArray[index];
                    var num = _context.Parameters.BufferWidth - 1;
                    if ((num > adjustedPrefixWidth) & (nonNullMessage.Length + adjustedPrefixWidth > num) && _context.Parameters.AlignMessages)
                    {
                        var str = nonNullMessage.Replace("\t", "        ");
                        var startIndex = 0;
                        var length1 = str.Length;
                        while (startIndex < length1)
                        {
                            var length2 = length1 - startIndex < num - adjustedPrefixWidth ? length1 - startIndex : num - adjustedPrefixWidth;
                            WriteBasedOnPrefix(str.Substring(startIndex, length2), prefixAlreadyWritten && startIndex == 0 && index == 0, adjustedPrefixWidth);
                            startIndex += length2;
                        }
                    }
                    else
                    {
                        WriteBasedOnPrefix(nonNullMessage, prefixAlreadyWritten, adjustedPrefixWidth);
                    }
                }
            }
        }

        public void PrintMessage(BuildMessageEventArgs e, bool lightenText)
        {
            var message = e.File == null ? (e.Message ?? string.Empty) : _eventFormatter.FormatEventMessage(e, false, _context.Parameters.ShowProjectFile);
            var prefixAdjustment = 0;
            if (e.BuildEventContext.TaskId != -1 && e.File == null)
            {
                prefixAdjustment = 2;
            }

            if (lightenText)
            {
                _logWriter.SetColor(Color.Details);
            }

            PrintTargetNamePerMessage(e, lightenText);
            if ((_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false)) && e.BuildEventContext.TaskId != -1)
            {
                var prefixAlreadyWritten = WriteTargetMessagePrefix(e, e.BuildEventContext, e.Timestamp);
                WriteMessageAligned(_stringService.FormatResourceString("TaskMessageWithId", message, e.BuildEventContext.TaskId), prefixAlreadyWritten, prefixAdjustment);
            }
            else if (_context.Parameters.ShowTimeStamp || _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                var prefixAlreadyWritten = WriteTargetMessagePrefix(e, e.BuildEventContext, e.Timestamp);
                WriteMessageAligned(message, prefixAlreadyWritten, prefixAdjustment);
            }
            else
            {
                WriteMessageAligned(message, false, prefixAdjustment);
            }

            if (!lightenText)
            {
                return;
            }

            _logWriter.ResetColor();
        }

        public void WriteNewLine()
        {
            _logWriter.Write(Environment.NewLine);
        }

        public bool WriteTargetMessagePrefix(BuildEventArgs e, BuildEventContext context, DateTime timeStamp)
        {
            var flag = true;
            var fullProjectKey = _context.GetFullProjectKey(e.BuildEventContext);
            if (!_context.LastProjectFullKey.Equals(fullProjectKey))
            {
                WriteLinePrefix(context, timeStamp, false);
                _context.LastProjectFullKey = fullProjectKey;
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        public void DisplayCounters(IDictionary<string, IPerformanceCounter> counters)
        {
            var sortedCounters = new List<IPerformanceCounter>(counters.Values);
            sortedCounters.Sort(DescendingByElapsedTime.Shared);
            var hasReenteredScope = false;
            foreach (var performanceCounter in sortedCounters)
            {
                if (performanceCounter.ReenteredScope)
                {
                    hasReenteredScope = true;
                }

                performanceCounter.PrintCounterMessage();
            }

            if (!hasReenteredScope)
            {
                return;
            }

            WriteLinePrettyFromResource(4, "PerformanceReentrancyNote");
        }

        public void WriteLinePrettyFromResource(string resourceString, params object[] args)
        {
            if (resourceString == null) throw new ArgumentNullException(nameof(resourceString));
            if (args == null) throw new ArgumentNullException(nameof(args));
            WriteLinePrettyFromResource(_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) ? _context.CurrentIndentLevel : 0, resourceString, args);
        }

        public void WriteLinePrettyFromResource(int indentLevel, string resourceString, params object[] args)
        {
            if (resourceString == null) throw new ArgumentNullException(nameof(resourceString));
            if (args == null) throw new ArgumentNullException(nameof(args));
            var formattedString = _stringService.FormatResourceString(resourceString, args);
            WriteLinePretty(indentLevel, formattedString);
        }

        public void WriteLinePretty(string formattedString)
        {
            if (formattedString == null) throw new ArgumentNullException(nameof(formattedString));
            WriteLinePretty(_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) ? _context.CurrentIndentLevel : 0, formattedString);
        }

        public void WriteLinePretty(int indentLevel, string formattedString)
        {
            if (formattedString == null) throw new ArgumentNullException(nameof(formattedString));
            indentLevel = indentLevel > 0 ? indentLevel : 0;
            _logWriter.Write(IndentString(formattedString, indentLevel * 2));
        }

        private void WriteBasedOnPrefix(string nonNullMessage, bool prefixAlreadyWritten, int adjustedPrefixWidth)
        {
            if (prefixAlreadyWritten)
            {
                _logWriter.Write(nonNullMessage + Environment.NewLine);
            }
            else
            {
                _logWriter.Write(IndentString(nonNullMessage, adjustedPrefixWidth));
            }
        }

        private static string[] SplitStringOnNewLines([NotNull] string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            return str.Split(NewLines, StringSplitOptions.None);
        }

        private void WritePretty([NotNull] string formattedString)
        {
            if (formattedString == null) throw new ArgumentNullException(nameof(formattedString));
            WritePretty(_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) ? _context.CurrentIndentLevel : 0, formattedString);
        }

        private void WritePretty(int indentLevel, [NotNull] string formattedString)
        {
            if (formattedString == null) throw new ArgumentNullException(nameof(formattedString));
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(' ', indentLevel * 2).Append(formattedString);
            _logWriter.Write(stringBuilder.ToString());
        }

        public string IndentString(string str)
        {
            return IndentString(str, _context.PrefixWidth);
        }

        private static string IndentString([CanBeNull] string str, int indent)
        {
            if (str == null)
            {
                str = string.Empty;
            }

            var lines = SplitStringOnNewLines(str);
            var stringBuilder = new StringBuilder(lines.Length * indent + lines.Length * Environment.NewLine.Length + str.Length);
            foreach (var line in lines)
            {
                stringBuilder.Append(' ', indent).Append(line);
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        private void PrintTargetNamePerMessage(BuildEventArgs e, bool lightenText)
        {
            if (!_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                return;
            }

            var buildEventContext = e.BuildEventContext;
            var flag1 = false;
            var str = string.Empty;
            var flag2 = ComparerContextNodeIdTargetId.Shared.Equals(buildEventContext, _context.LastDisplayedBuildEventContext == (BuildEventContext)null ? null : _context.LastDisplayedBuildEventContext);
            TargetStartedEventMinimumFields eventMinimumFields = null;
            if (!flag2)
            {
                eventMinimumFields = _buildEventManager.GetTargetStartedEvent(buildEventContext);
                if (eventMinimumFields != null)
                {
                    str = eventMinimumFields.TargetName;
                    flag1 = true;
                }
            }

            if (!flag1)
            {
                return;
            }

            var prefixAlreadyWritten = WriteTargetMessagePrefix(e, eventMinimumFields.TargetBuildEventContext, eventMinimumFields.TimeStamp);
            _logWriter.SetColor(Color.BuildStage);
            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
            {
                WriteMessageAligned(_stringService.FormatResourceString("TargetMessageWithId", (object)str, (object)e.BuildEventContext.TargetId), prefixAlreadyWritten);
            }
            else
            {
                WriteMessageAligned(str + ":", prefixAlreadyWritten);
            }

            if (lightenText)
            {
                _logWriter.SetColor(Color.Details);
            }
            else
            {
                _logWriter.ResetColor();
            }
        }

        public void WriteLinePrefix(string key, DateTime eventTimeStamp, bool isMessagePrefix)
        {
            if (_context.NumberOfProcessors == 1)
            {
                return;
            }

            _logWriter.SetColor(Color.BuildStage);
            var str = string.Empty;
            if (_context.Parameters.ShowTimeStamp || _context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
            {
                str = _logFormatter.FormatLogTimeStamp(eventTimeStamp);
            }

            string formattedString;
            if (!isMessagePrefix || _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                formattedString = _stringService.FormatResourceString("BuildEventContext", str, key) + ">";
            }
            else
            {
                formattedString = _stringService.FormatResourceString("BuildEventContext", str, string.Empty) + " ";
            }

            WritePretty(formattedString);
            _logWriter.ResetColor();
            if (_context.PrefixWidth != 0)
            {
                return;
            }

            _context.PrefixWidth = formattedString.Length;
        }
    }
}
