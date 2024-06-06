using System.Diagnostics;
using FluentAssertions;
using Messages;
using Moq;
using Xunit.Abstractions;

namespace MessagesTest;

public class MessageParserTest
{
    private readonly ITestOutputHelper _output;

    public MessageParserTest(ITestOutputHelper output)
    {
        _output = output;
        var traceListener = new XunitTraceListener(output);
        Trace.Listeners.Add(traceListener);
    }

    [Fact]
    public void ReadText_CanReadMessageHeader()
    {
        var reader = new StringReader("H|^|1|2|3|4|5|6|7|8|9|10\n");
        var spec = MessageParserSpec.Default;
        var parser = new MessageParser(spec);

        var message = parser.ReadText(reader);

        message.Should().NotBeNull();
        message.Records.ToArray().Length.Should().Be(1);
        var segment = message.Records.First();
        segment.Components.Count.Should().Be(11);
        segment.Label.Should().Be("H");
        segment.Components[0].Value().Should().Be("^");
        segment.Components[1].Value().Should().Be("1");
        segment.Components[10].Value().Should().Be("10");
    }

    [Fact]
    public void ReadText_CanReadMessageWithMultipleSegments()
    {
        var reader = new StringReader(
            """
            H|^|1|2|3|4|5|6|7|8|9|10
            P|a^b|c^d|e^f
            P|G&H^I&J|K&L^M&N
            """);
        var parser = new MessageParser(MessageParserSpec.Default);

        var message = parser.ReadText(reader);

        message.Records.ToArray().Length.Should().Be(3);
        var segments = message.Records.ToArray();

        segments[0].Label.Should().Be("H");
        segments[0].Components.Count.Should().Be(11);
        segments[0].Components[0].Value().Should().Be("^");
        segments[0].Components[1].Value().Should().Be("1");
        segments[0].Components[10].Value().Should().Be("10");

        segments[1].Label.Should().Be("P");
        segments[1].Components.Count.Should().Be(3);
        segments[1].Components[0].Value().Should().Be("a^b");
        segments[1].Components[1].Value().Should().Be("c^d");
        segments[1].Components[2].Value().Should().Be("e^f");

        segments[2].Label.Should().Be("P");
        segments[2].Components.Count.Should().Be(2);
        segments[2].Components[0].Value().Should().Be("G&H^I&J");
        segments[2].Components[1].Value().Should().Be("K&L^M&N");
    }

    [Theory]
    [InlineData("^~\\&")]
    [InlineData("^~\\&#")]
    public void ReadText_CanReadHl7StyleMessage(string specialChars)
    {
        var reader = new StringReader(
            $"""
            MSH|{specialChars}|1|2|3|4|5|6|7|8|9|10
            PID|a^b|c^d|e^f
            PID|G&H^I&J|K&L^M&N
            """);
        var parser = new MessageParser(new Hl7TestMessageParserSpec());

        var message = parser.ReadText(reader);

        message.Records.ToArray().Length.Should().Be(3);
        var segments = message.Records.ToArray();

        segments[0].Label.Should().Be("MSH");
        segments[0].Components.Count.Should().Be(11);
        segments[0].Components[0].Value().Should().Be(specialChars);
        segments[0].Components[1].Value().Should().Be("1");
        segments[0].Components[10].Value().Should().Be("10");

        segments[1].Label.Should().Be("PID");
        segments[1].Components.Count.Should().Be(3);
        segments[1].Components[0].Value().Should().Be("a^b");
        segments[1].Components[1].Value().Should().Be("c^d");
        segments[1].Components[2].Value().Should().Be("e^f");

        segments[2].Label.Should().Be("PID");
        segments[2].Components.Count.Should().Be(2);
        segments[2].Components[0].Value().Should().Be("G&H^I&J");
        segments[2].Components[1].Value().Should().Be("K&L^M&N");
    }
}
