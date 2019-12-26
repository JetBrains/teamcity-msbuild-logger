namespace TeamCity.MSBuild.Logger.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using JetBrains.TeamCity.ServiceMessages;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using Shouldly;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    internal static class ServiceMessages
    {
        private static readonly IServiceMessageParser Parser = new ServiceMessageParser();

        [IoC.NotNull]
        public static IEnumerable<string> FilterTeamCityServiceMessages([IoC.NotNull] IEnumerable<string> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            foreach (var line in lines)
            {
                if (line != null && line.StartsWith("##teamcity["))
                {
                    continue;
                }

                yield return line;
            }
        }

        public static int GetNumberServiceMessage([IoC.NotNull] IEnumerable<string> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            return GetNumberServiceMessage(string.Join("\n", lines));
        }

        public static void ResultShouldContainCorrectStructureAndSequence([IoC.NotNull] IEnumerable<string> lines)
        {
            ResultShouldContainCorrectStructureAndSequence(string.Join("\n", lines));
        }

        private static int GetNumberServiceMessage([IoC.NotNull] string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var actualMessages = Parser.ParseServiceMessages(text).ToList();
            return actualMessages.Count;
        }

        private static void ResultShouldContainCorrectStructureAndSequence([IoC.NotNull] string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var messages = Parser.ParseServiceMessages(text).ToList();
            var rootFlows = new List<Flow>();
            foreach (var serviceMessage in messages)
            {
                var message = new Message(serviceMessage);
                var flow = rootFlows.SingleOrDefault(i => i.FlowId == message.FlowIdAttr);
                if (flow == null)
                {
                    flow = new Flow(message.CurrentFlowId);
                    rootFlows.Add(flow);
                }

                flow.ProcessMessage(message);

                if (flow.IsFinished)
                {
                    rootFlows.Remove(flow);
                }
            }
        }

        private class Message
        {
            public Message(IServiceMessage message)
            {
                if (message == null) throw new ArgumentNullException(nameof(message));
                Name = message.Name;
                FlowIdAttr = message.GetValue("flowId") ?? string.Empty;
                NameAttr = message.GetValue("name");
                ParentAttr = message.GetValue("parent");
                CaptureStandardOutputAttr = message.GetValue("captureStandardOutput");
                DurationAttr = message.GetValue("duration");
                OutAttr = message.GetValue("duration");
                MessageAttr = message.GetValue("message");
                DetailsAttr = message.GetValue("details");
                TcTagsAttr = message.GetValue("tc:tags");
                IdentityAttr = message.GetValue("identity");
                DescriptionAttr = message.GetValue("description");
                KeyAttr = message.GetValue("key");
                ValueAttr = message.GetValue("value");
            }

            public string Name { get; }

            public string FlowIdAttr { get; }

            public string NameAttr { get; }

            public string ParentAttr { get; }

            public string CurrentFlowId => ParentAttr ?? FlowIdAttr;

            public string CaptureStandardOutputAttr { get; }

            public string DurationAttr { get; }

            public string OutAttr { get; }

            public string MessageAttr { get; }

            public string DetailsAttr { get; }

            public string TcTagsAttr { get; }

            public string IdentityAttr { get; }

            public string DescriptionAttr { get; }

            public string KeyAttr { get; }

            public string ValueAttr { get; }
        }

        private class Flow
        {
            private readonly Stack<Message> _messages = new Stack<Message>();
            private readonly HashSet<string> _statistics = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            public Flow(string flowId)
            {
                FlowId = flowId;
            }

            public string FlowId { get; private set; }

            public bool IsFinished => _messages.Count == 0;

            public void ProcessMessage(Message message)
            {
                switch (message.Name)
                {
                    case "message":
                        break;

                    case "blockOpened":
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        FlowId = message.FlowIdAttr;
                        _messages.Push(message);
                        break;

                    case "blockClosed":
                        var blockClosed = _messages.Pop();
                        AreEqual(blockClosed.Name, "blockOpened", "blockOpened should close blockClosed");
                        AreEqual(blockClosed.NameAttr, message.NameAttr, "Invalid Name attribute");
                        break;

                    case "testSuiteStarted":
                        IsNotEmpty(message.FlowIdAttr, "FlowId attribute is empty");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        FlowId = message.FlowIdAttr;
                        _messages.Push(message);
                        break;

                    case "testSuiteFinished":
                        var testSuiteStarted = _messages.Pop();
                        AreEqual(testSuiteStarted.Name, "testSuiteStarted", "testSuiteFinished should close testSuiteStarted");
                        AreEqual(testSuiteStarted.FlowIdAttr, message.FlowIdAttr, "Invalid FlowId attribute");
                        AreEqual(testSuiteStarted.NameAttr, message.NameAttr, "Invalid Name attribute");
                        break;

                    case "flowStarted":
                        IsNotEmpty(message.FlowIdAttr, "Invalid FlowId attribute");
                        AreEqual(message.ParentAttr, FlowId, "Invalid Parent attribute");
                        FlowId = message.FlowIdAttr;
                        _messages.Push(message);
                        break;

                    case "flowFinished":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        Greater(_messages.Count, 0, "flowFinished should close flowStarted");
                        var flowStarted = _messages.Pop();
                        AreEqual(flowStarted.Name, "flowStarted", "flowFinished should close flowStarted");
                        FlowId = flowStarted.ParentAttr;
                        break;

                    case "testStarted":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        if (message.CaptureStandardOutputAttr != null)
                            AreEqual(message.CaptureStandardOutputAttr, "false", "Invalid CaptureStandardOutput attribute");

                        _messages.Push(message);
                        break;

                    case "testFinished":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        Greater(_messages.Count, 0, "testFinished should close testStarted");
                        var testStarted = _messages.Pop();
                        AreEqual(testStarted.Name, "testStarted", "testFinished should close testStarted");
                        AreEqual(testStarted.NameAttr, message.NameAttr, "Invalid Name attribute");
                        IsNotEmpty(message.DurationAttr, "Duration attribute is empty");
                        break;

                    case "testStdOut":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        Greater(_messages.Count, 1, "testStdOut should be within testStarted and testFinished");
                        var testStartedForStdOut = _messages.Peek();
                        AreEqual(testStartedForStdOut.Name, "testStarted", "testStdOut should be within testStarted and testFinished");
                        AreEqual(testStartedForStdOut.NameAttr, message.NameAttr, "Invalid Name attribute");
                        IsNotEmpty(message.OutAttr, "Out attribute is empty");
                        IsNotEmpty(message.TcTagsAttr, "tc:tags should be tc:parseServiceMessagesInside");
                        break;

                    case "testFailed":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        Greater(_messages.Count, 1, "testFailed should be within testStarted and testFinished");
                        var testStartedForTestFailed = _messages.Peek();
                        AreEqual(testStartedForTestFailed.Name, "testStarted", "testFailed should be within testStarted and testFinished");
                        AreEqual(testStartedForTestFailed.NameAttr, message.NameAttr, "Invalid Name attribute");
                        // IsNotEmpty(message.MessageAttr, "Message attribute is empty");
                        IsNotNull(message.DetailsAttr, "Details attribute is empty");
                        break;

                    case "testIgnored":
                        AreEqual(message.FlowIdAttr, FlowId, "Invalid FlowId attribute");
                        IsNotEmpty(message.NameAttr, "Name attribute is empty");
                        Greater(_messages.Count, 1, "testIgnored should be within testStarted and testFinished");
                        var testStartedForTestIgnored = _messages.Peek();
                        AreEqual(testStartedForTestIgnored.Name, "testStarted", "testIgnored should be within testStarted and testFinished");
                        AreEqual(testStartedForTestIgnored.NameAttr, message.NameAttr, "Invalid Name attribute");
                        // IsNotEmpty(message.MessageAttr, "Message attribute is empty");
                        break;

                    case "buildProblem":
                        IsNotEmpty(message.IdentityAttr, "Identity attribute is empty");
                        IsNotEmpty(message.DescriptionAttr, "Description attribute is empty");
                        break;

                    case "buildStatisticValue":
                        IsNotEmpty(message.KeyAttr, "Key attribute is empty");
                        IsNotEmpty(message.ValueAttr, "Value attribute is empty");
                        if (!_statistics.Add(message.KeyAttr))
                        {
                            Fail($"Statistics {message.KeyAttr} already exists");
                        }

                        break;

                    default:
                        Fail($"Unexpected message {message.Name}");
                        break;
                }
            }
        }

        private static void Fail(string message)
        {
            throw new Exception(message);
        }

        private static void IsNotNull(string str, string message)
        {
            str.ShouldNotBeNull(message);
        }

        private static void Greater(int val1, int val2, string message)
        {
            val1.ShouldBeGreaterThan(val2, message);
        }

        private static void IsNotEmpty(string str, string message = "")
        {
            str.ShouldNotBeNullOrEmpty(message);
        }

        private static void AreEqual<T>(T val1, T val2, string message)
        {
            val1.ShouldBe(val2, message);
        }
    }
}