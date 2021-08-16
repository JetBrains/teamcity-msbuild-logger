namespace TeamCity.MSBuild.Logger.Tests
{
    using System.Linq;
    using JetBrains.Annotations;
    using JetBrains.TeamCity.ServiceMessages;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using Moq;
    using Xunit;

    public class TeamCityHierarchicalMessageWriterTests
    {
        private readonly Mock<IColorTheme> _colorTheme;
        private readonly Mock<ITeamCityWriter> _rootWriter;
        private readonly Mock<IColorStorage> _colorStorage;
        private readonly Mock<IServiceMessageParser> _serviceMessageParser;
        private readonly Mock<IEventContext> _eventContext;

        public TeamCityHierarchicalMessageWriterTests()
        {
            _colorTheme = new Mock<IColorTheme>();
            _rootWriter = new Mock<ITeamCityWriter>();
            _colorStorage = new Mock<IColorStorage>();
            _serviceMessageParser = new Mock<IServiceMessageParser>();
            _eventContext = new Mock<IEventContext>();
            Color? currentColor = null;
            _colorStorage.Setup(i => i.SetColor(It.IsAny<Color>())).Callback<Color>(i => currentColor = i);
            _colorStorage.Setup(i => i.ResetColor()).Callback(() => currentColor = null);
            _colorStorage.SetupGet(i => i.Color).Returns(() => currentColor);
        }

        [Fact]
        public void ShouldNotCloseMainFlow()
        {
            // Given
            var writer = CreateInstance();
            writer.Write("my");

            // When
            writer.Dispose();

            // Then
            _rootWriter.Verify(i => i.Dispose(), Times.Never);
        }

        [Fact]
        public void ShouldSendMessageInDefaultFlow()
        {
            // Given
            var writer = CreateInstance();

            // When
            writer.Write("my message\n");

            // Then
            _rootWriter.Verify(i => i.WriteMessage("my message"), Times.Once());
        }

        [Theory]
        [InlineData("##teamcity[abc")]
        [InlineData("##teamcity[abc]  ")]
        [InlineData(" ##teamcity[abc")]
        [InlineData("    ##teamcity[abc")]
        [InlineData(" ##TeamCity[Abc")]
        public void ShouldNotCreateNestedServiceMessage(string serviceMessage)
        {
            // Given
            var writer = CreateInstance(new Parameters {PlainServiceMessage = true});
            var serviceMessage1 = new ServiceMessage("message");
            var serviceMessage2 = new ServiceMessage("publishArtifacts");
            _serviceMessageParser.Setup(i => i.ParseServiceMessages(serviceMessage.Trim())).Returns(new IServiceMessage[] { serviceMessage1, serviceMessage2 });

            // When
            writer.Write(serviceMessage + "\n");

            // Then
            _rootWriter.Verify(i => i.WriteMessage(It.IsAny<string>()), Times.Never);
            _rootWriter.Verify(i => i.WriteRawMessage(It.IsAny<IServiceMessage>()), Times.Exactly(2));
        }

        [Fact]
        public void ShouldSendMessageAsIsWhenCannotParseAnyServiceMessages()
        {
            // Given
            var writer = CreateInstance();
            const string serviceMessage = "##teamcity[abc]";
            _serviceMessageParser.Setup(i => i.ParseServiceMessages(serviceMessage.Trim())).Returns(Enumerable.Empty<IServiceMessage>());

            // When
            writer.Write(serviceMessage + "\n");

            // Then
            _rootWriter.Verify(i => i.WriteMessage(serviceMessage), Times.Once());
        }

        [Fact]
        public void ShouldSendWarning()
        {
            // Given
            var writer = CreateInstance();
            writer.SetColor(Color.Warning);

            // When
            writer.Write("my");
            writer.Write(" warning\n");

            // Then
            _rootWriter.Verify(i => i.WriteWarning("my warning"), Times.Once());
        }

        [Fact]
        public void ShouldSendWarningSummaryAsWarning()
        {
            // Given
            var writer = CreateInstance();
            writer.SetColor(Color.WarningSummary);

            // When
            writer.Write("my");
            writer.Write(" warning\n");

            // Then
            _rootWriter.Verify(i => i.WriteWarning("my warning"), Times.Once());
        }

        [Fact]
        public void ShouldSetColor()
        {
            // Given
            _colorTheme.Setup(i => i.GetAnsiColor(Color.Details)).Returns("color#");
            var writer = CreateInstance();
            writer.SetColor(Color.Details);

            // When
            writer.Write("my");
            writer.Write(" error\n");

            // Then
            _rootWriter.Verify(i => i.WriteError("color#my error", null), Times.Never);
        }

        [Fact]
        public void ShouldSetColorInTheMiddleOfText()
        {
            // Given
            _colorTheme.Setup(i => i.GetAnsiColor(Color.Details)).Returns("color#");
            var writer = CreateInstance();

            // When
            writer.Write("my");
            writer.SetColor(Color.Details);
            writer.Write(" error\n");

            // Then
            _rootWriter.Verify(i => i.WriteError("color#my error", null), Times.Never);
        }

        [Fact]
        public void ShouldResetColorInTheMiddleOfText()
        {
            // Given
            _colorTheme.Setup(i => i.GetAnsiColor(Color.Details)).Returns("color#");
            var writer = CreateInstance();
            writer.SetColor(Color.Details);

            // When
            writer.Write("my");
            writer.ResetColor();
            writer.Write(" error\n");

            // Then
            _rootWriter.Verify(i => i.WriteError("my error", null), Times.Never);
        }

        [Fact]
        public void ShouldResetStateColor()
        {
            // Given
            var writer = CreateInstance();
            writer.SetColor(Color.Error);

            // When
            writer.Write("my");
            writer.ResetColor();
            writer.Write(" error\n");

            // Then
            _rootWriter.Verify(i => i.WriteError("my error", null), Times.Never);
        }

        [Fact]
        public void ShouldSendError()
        {
            // Given
            var writer = CreateInstance();
            writer.SetColor(Color.Error);

            // When
            writer.Write("my");
            writer.Write(" error\n");

            // Then
            _rootWriter.Verify(i => i.WriteError("my error", null), Times.Once());
        }

        [Fact]
        public void ShouldSendBuildProblemWhenFinished()
        {
            // Given
            var writer = CreateInstance();
            writer.SetColor(Color.ErrorSummary);

            // When
            writer.Write("my");
            writer.Write(" error\n");
            writer.Dispose();

            // Then
            _rootWriter.Verify(i => i.WriteBuildProblem("msbuild", "my error"), Times.Once());
        }

        [Fact]
        public void ShouldNotSendNullMessage()
        {
            // Given
            var writer = CreateInstance();

            // When
            writer.Write(null);

            // Then
            _rootWriter.Verify(i => i.WriteMessage(It.IsAny<string>()), Times.Never);
            _rootWriter.Verify(i => i.Dispose(), Times.Never);
        }

        [Fact]
        public void ShouldNotSendEmptyMessage()
        {
            // Given
            var writer = CreateInstance();

            // When
            writer.Write(string.Empty);

            // Then
            _rootWriter.Verify(i => i.WriteMessage(It.IsAny<string>()), Times.Never);
            _rootWriter.Verify(i => i.Dispose(), Times.Never);
        }

        [Fact]
        public void ShouldCombineMessageUntilEndOfLine()
        {
            // Given
            var writer = CreateInstance();

            // When
            writer.Write("my");
            writer.Write(" message\n");

            // Then
            _rootWriter.Verify(i => i.WriteMessage("my message"), Times.Once());
        }

        [Fact]
        public void ShouldSendBufferedMessagesAfterFinishForMainFlow()
        {
            // Given
            var writer = CreateInstance();
            writer.Write("my");

            // When
            writer.Dispose();

            // Then
            _rootWriter.Verify(i => i.WriteMessage("my"), Times.Once());
        }

        [Fact]
        public void ShouldSendBufferedMessagesAfterFinishForOtherFlow()
        {
            // Given
            var writer = CreateInstance();

            // When
            using (new HierarchicalContext(2))
            {
                writer.Write("my");
                writer.Dispose();
            }

            // Then
            _rootWriter.Verify(i => i.WriteMessage("my"), Times.Once());
        }

        [Fact]
        public void ShouldCreateBlocksInDefaultFlow()
        {
            // Given
            var writer = CreateInstance();
            var block1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            writer.StartBlock("block1");
            writer.Write("my");
            writer.Write(" message\n");
            writer.Write("abc\n");
            writer.FinishBlock();
            writer.Write("zzz\n");

            // Then
            block1Writer.Verify(i => i.WriteMessage("my message"), Times.Once());
            block1Writer.Verify(i => i.WriteMessage("abc"), Times.Once());
            _rootWriter.Verify(i => i.WriteMessage("zzz"), Times.Once());
        }

        [Fact]
        public void ShouldSendBufferedMessageWhenBlockClosedWhenMainFlow()
        {
            // Given
            var writer = CreateInstance();
            var block1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            writer.StartBlock("block1");
            writer.Write("my");
            writer.FinishBlock();
            
            // Then
            block1Writer.Verify(i => i.WriteMessage("my"), Times.Once());
        }

        [Fact]
        public void ShouldSendBufferedMessageWhenBlockClosedWhenSomeFlow()
        {
            // Given
            var writer = CreateInstance();
            var flow1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow1Writer.Object);
            var block1Writer = new Mock<ITeamCityWriter>();
            flow1Writer.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            using (new HierarchicalContext(10))
            {
                writer.StartBlock("block1");
                writer.Write("my");
                writer.FinishBlock();
            }

            // Then
            block1Writer.Verify(i => i.WriteMessage("my"), Times.Once());
        }

        [Fact]
        public void ShouldCreateBlocksInNewFlow()
        {
            // Given
            var writer = CreateInstance();
            var flow1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow1Writer.Object);
            var block1Writer = new Mock<ITeamCityWriter>();
            flow1Writer.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            using (new HierarchicalContext(10))
            {
                writer.StartBlock("block1");
                writer.Write("my");
                writer.Write(" message\n");
                writer.Write("abc\n");
                writer.FinishBlock();
            }

            // Then
            _rootWriter.Verify(i => i.OpenFlow(), Times.Once());
            block1Writer.Verify(i => i.WriteMessage("my message"), Times.Once());
            block1Writer.Verify(i => i.WriteMessage("abc"), Times.Once());
        }

        [Fact]
        public void ShouldCloseFlowWhenBlockClosed()
        {
            // Given
            var writer = CreateInstance();
            var flow1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow1Writer.Object);
            var flow2Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow2Writer.Object);
            var block1Writer = new Mock<ITeamCityWriter>();
            flow2Writer.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            using (new HierarchicalContext(2))
            {
                writer.Write("abc 2\n");
                using (new HierarchicalContext(3))
                {
                    writer.StartBlock("block1");
                    writer.Write("abc 3\n");
                    writer.FinishBlock();
                }
            }

            // Then
            _rootWriter.Verify(i => i.OpenFlow(), Times.Exactly(1));
            flow1Writer.Verify(i => i.Dispose(), Times.Never);
            flow2Writer.Verify(i => i.Dispose(), Times.Once);
        }

        [Fact]
        public void ShouldCloseFlowWhenHasNoClosedBlock()
        {
            // Given
            var writer = CreateInstance();
            var flow1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow1Writer.Object);
            var flow2Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow2Writer.Object);
            var block1Writer = new Mock<ITeamCityWriter>();
            flow2Writer.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);
            var block2Writer = new Mock<ITeamCityWriter>();
            block1Writer.Setup(i => i.OpenBlock("block2")).Returns(block2Writer.Object);

            // When
            using (new HierarchicalContext(2))
            {
                writer.Write("abc 2\n");
                using (new HierarchicalContext(3))
                {
                    writer.StartBlock("block1");
                    writer.Write("abc 3\n");
                    writer.StartBlock("block2");
                }
            }

            writer.FinishBlock();

            // Then
            _rootWriter.Verify(i => i.OpenFlow(), Times.Exactly(1));
            flow1Writer.Verify(i => i.Dispose(), Times.Never);
            flow2Writer.Verify(i => i.Dispose(), Times.Never);
        }

        [Fact]
        public void ShouldNotThrowExceptionWhenBlocClosedTooManyTimes()
        {
            // Given
            var writer = CreateInstance();
            var flow2Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow2Writer.Object);
            var block1Writer = new Mock<ITeamCityWriter>();
            flow2Writer.Setup(i => i.OpenBlock("block1")).Returns(block1Writer.Object);

            // When
            using (new HierarchicalContext(3))
            {
                writer.StartBlock("block1");
                writer.Write("abc 3\n");
                writer.FinishBlock();
                writer.FinishBlock();
                writer.FinishBlock();
                writer.FinishBlock();
            }

            // Then
            _rootWriter.Verify(i => i.OpenFlow(), Times.Once);
            flow2Writer.Verify(i => i.Dispose(), Times.Once);
        }

        [Fact]
        public void ShouldNotOpenFlowWhenNoBlocks()
        {
            // Given
            var writer = CreateInstance();
            var flow1Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow1Writer.Object);
            var flow2Writer = new Mock<ITeamCityWriter>();
            _rootWriter.Setup(i => i.OpenFlow()).Returns(flow2Writer.Object);

            // When
            using (new HierarchicalContext(2))
            {
                writer.Write("abc 2\n");
            }

            using (new HierarchicalContext(3))
            {
                writer.Write("abc 3\n");
            }

            // Then
            _rootWriter.Verify(i => i.OpenFlow(), Times.Never);
            _rootWriter.Verify(i => i.WriteMessage("abc 2"), Times.Once);
            _rootWriter.Verify(i => i.WriteMessage("abc 3"), Times.Once);
        }

        private TeamCityHierarchicalMessageWriter CreateInstance([CanBeNull] Parameters parameters = null)
        {
            var loggerContext = new Mock<ILoggerContext>();
            loggerContext.SetupGet(i => i.Parameters).Returns(parameters ?? new Parameters());
            return new TeamCityHierarchicalMessageWriter(
                loggerContext.Object,
                _colorTheme.Object,
                _rootWriter.Object,
                _serviceMessageParser.Object,
                _colorStorage.Object,
                _eventContext.Object);
        }
    }
}
