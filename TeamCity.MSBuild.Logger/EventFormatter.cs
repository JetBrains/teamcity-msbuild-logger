namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Globalization;
    using System.Text;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EventFormatter : IEventFormatter
    {
        private static readonly string[] NewLines = {"\r\n", "\n"};

        public string FormatEventMessage(
            BuildErrorEventArgs e,
            bool removeCarriageReturn,
            bool showProjectFile)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            return FormatEventMessage(
                "error",
                e.Subcategory,
                removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message,
                e.Code,
                e.File,
                showProjectFile ? e.ProjectFile : null,
                e.LineNumber,
                e.EndLineNumber,
                e.ColumnNumber,
                e.EndColumnNumber,
                e.ThreadId);
        }

        public string FormatEventMessage(
            BuildWarningEventArgs e,
            bool removeCarriageReturn,
            bool showProjectFile)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            return FormatEventMessage(
                "warning",
                e.Subcategory,
                removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message,
                e.Code,
                e.File,
                showProjectFile ? e.ProjectFile : null, e.LineNumber,
                e.EndLineNumber,
                e.ColumnNumber,
                e.EndColumnNumber,
                e.ThreadId);
        }

        public string FormatEventMessage(BuildMessageEventArgs e, bool removeCarriageReturn, bool showProjectFile)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            return FormatEventMessage(
                "message",
                e.Subcategory,
                removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message,
                e.Code,
                e.File,
                showProjectFile ? e.ProjectFile : null,
                e.LineNumber,
                e.EndLineNumber,
                e.ColumnNumber,
                e.EndColumnNumber,
                e.ThreadId);
        }

        private static string FormatEventMessage(
            string category,
            string subcategory,
            string message,
            string code,
            string file,
            string projectFile,
            int lineNumber,
            int endLineNumber,
            int columnNumber,
            int endColumnNumber,
            int threadId)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(file))
            {
                sb.Append("MSBUILD : ");
            }
            else
            {
                sb.Append("{1}");
                if (lineNumber == 0)
                {
                    sb.Append(" : ");
                }
                else if (columnNumber == 0)
                {
                    sb.Append(endLineNumber == 0 ? "({2}): " : "({2}-{7}): ");
                }
                else if (endLineNumber == 0)
                {
                    sb.Append(endColumnNumber == 0 ? "({2},{3}): " : "({2},{3}-{8}): ");
                }
                else if (endColumnNumber == 0)
                {
                    sb.Append("({2}-{7},{3}): ");
                }
                else
                {
                    sb.Append("({2},{3},{7},{8}): ");
                }
            }

            if (!string.IsNullOrEmpty(subcategory))
            {
                sb.Append("{9} ");
            }

            sb.Append("{4} ");
            sb.Append(code == null ? ": " : "{5}: ");

            if (message != null)
            {
                sb.Append("{6}");
            }

            if (projectFile != null && !string.Equals(projectFile, file))
            {
                sb.Append(" [{10}]");
            }

            if (message == null)
            {
                message = string.Empty;
            }

            var format = sb.ToString();
            var strArray = SplitStringOnNewLines(message);
            var stringBuilder2 = new StringBuilder();
            for (var index = 0; index < strArray.Length; ++index)
            {
                stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, format, threadId,
                    file, lineNumber, columnNumber, category, code,
                    strArray[index], endLineNumber, endColumnNumber, subcategory,
                    projectFile));

                if (index < strArray.Length - 1)
                {
                    stringBuilder2.AppendLine();
                }
            }
            return stringBuilder2.ToString();
        }

        private static string[] SplitStringOnNewLines(string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            return str.Split(NewLines, StringSplitOptions.None);
        }

        private static string EscapeCarriageReturn(string stringWithCarriageReturn)
        {
            return !string.IsNullOrEmpty(stringWithCarriageReturn) ? stringWithCarriageReturn.Replace("\r", "\\r") : stringWithCarriageReturn;
        }
    }
}