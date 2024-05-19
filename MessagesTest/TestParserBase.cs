using MessageParser;
using Moq;
using FluentAssertions;

namespace MessagesTest;

public class TestParserBase
{
    [Fact]
    public void ReadText_CanReadMessageWithSingleSegment()
    {
        var reader = new StringReader("MSH|^~\\&|1|2|3|4|5|6|7|8|9|10\n");
        var parser = new Mock<ParserBase<RawMessage, RawSegment>> {CallBase = true};

        var message = parser.Object.ReadText(reader);

        message.Should().NotBeNull();
        message.Segments.ToArray().Length.Should().Be(1);
        var segment = message.Segments.First();
        segment.Fields.Count.Should().Be(12);
        segment.Label.Should().Be("MSH");
        segment.Fields[0].Value.Should().Be("MSH");
        segment.Fields[1].Value.Should().Be("^~\\&");
        segment.Fields[2].Value.Should().Be("1");
        segment.Fields[11].Value.Should().Be("10");
    }

    [Fact]
    public void ReadText_CanReadMessageWithMultipleSegments()
    {
        var reader = new StringReader(
            """
            MSH|^~\&|1|2|3|4|5|6|7|8|9|10
            PID|a^b|c^d|e^f
            PID|G&H^I&J|K&L^M&N
            """);
        var parser = new Mock<ParserBase<RawMessage, RawSegment>> {CallBase = true};

        var message = parser.Object.ReadText(reader);

        message.Segments.ToArray().Length.Should().Be(3);
        var segments = message.Segments.ToArray();

        segments[0].Label.Should().Be("MSH");
        segments[0].Fields.Count.Should().Be(12);
        segments[0].Fields[0].Value.Should().Be("MSH");
        segments[0].Fields[1].Value.Should().Be("^~\\&");
        segments[0].Fields[2].Value.Should().Be("1");
        segments[0].Fields[11].Value.Should().Be("10");

        segments[1].Label.Should().Be("PID");
        segments[1].Fields.Count.Should().Be(4);
        segments[1].Fields[0].Value.Should().Be("PID");
        segments[1].Fields[1].Value.Should().Be("a^b");
        segments[1].Fields[2].Value.Should().Be("c^d");
        segments[1].Fields[3].Value.Should().Be("e^f");

        segments[2].Label.Should().Be("PID");
        segments[2].Fields.Count.Should().Be(3);
        segments[2].Fields[0].Value.Should().Be("PID");
        segments[2].Fields[1].Value.Should().Be("G&H^I&J");
        segments[2].Fields[2].Value.Should().Be("K&L^M&N");
    }
}
