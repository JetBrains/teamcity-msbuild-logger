using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace TeamCity.MSBuild.Logger
{
    internal interface IMessageWriter
    {
        void DisplayCounters(IDictionary<string, IPerformanceCounter> counters);

        void PrintMessage(BuildMessageEventArgs e, bool lightenText);

        void WriteLinePrefix(BuildEventContext e, DateTime eventTimeStamp, bool isMessagePrefix);

        void WriteLinePrefix(string key, DateTime eventTimeStamp, bool isMessagePrefix);

        void WriteLinePretty(int indentLevel, [NotNull] string formattedString);

        void WriteLinePretty([NotNull] string formattedString);

        void WriteLinePrettyFromResource(int indentLevel, [NotNull] string resourceString, [NotNull] params object[] args);

        void WriteLinePrettyFromResource([NotNull] string resourceString, [NotNull] params object[] args);

        void WriteMessageAligned(string message, bool prefixAlreadyWritten, int prefixAdjustment = 0);

        void WriteNewLine();

        bool WriteTargetMessagePrefix(BuildEventArgs e, BuildEventContext context, DateTime timeStamp);

        string IndentString([CanBeNull] string str);
    }
}