using MessageParser;
using Moq;
using FluentAssertions;

namespace MessagesTest;

public class ParserBaseTest
{
    [Fact]
    public void ReadText_CanReadMessageHeader()
    {
        var reader = new StringReader("H|^|1|2|3|4|5|6|7|8|9|10\n");
        var parser = new Mock<ParserBase<RawMessage, RawSegment>> {CallBase = true};

        var message = parser.Object.ReadText(reader);

        message.Should().NotBeNull();
        message.Segments.ToArray().Length.Should().Be(1);
        var segment = message.Segments.First();
        segment.Fields.Count.Should().Be(11);
        segment.Label.Should().Be("H");
        segment.Fields[0].Text.Should().Be("^");
        segment.Fields[1].Text.Should().Be("1");
        segment.Fields[10].Text.Should().Be("10");
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
        var parser = new Mock<ParserBase<RawMessage, RawSegment>> {CallBase = true};

        var message = parser.Object.ReadText(reader);

        message.Segments.ToArray().Length.Should().Be(3);
        var segments = message.Segments.ToArray();

        segments[0].Label.Should().Be("H");
        segments[0].Fields.Count.Should().Be(11);
        segments[0].Fields[0].Text.Should().Be("^");
        segments[0].Fields[1].Text.Should().Be("1");
        segments[0].Fields[10].Text.Should().Be("10");

        segments[1].Label.Should().Be("P");
        segments[1].Fields.Count.Should().Be(3);
        segments[1].Fields[0].Text.Should().Be("a^b");
        segments[1].Fields[1].Text.Should().Be("c^d");
        segments[1].Fields[2].Text.Should().Be("e^f");

        segments[2].Label.Should().Be("P");
        segments[2].Fields.Count.Should().Be(2);
        segments[2].Fields[0].Text.Should().Be("G&H^I&J");
        segments[2].Fields[1].Text.Should().Be("K&L^M&N");
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
        var parser = new Mock<ParserBase<Hl7TestMessage, RawSegment>> {CallBase = true};

        var message = parser.Object.ReadText(reader);

        message.Segments.ToArray().Length.Should().Be(3);
        var segments = message.Segments.ToArray();

        segments[0].Label.Should().Be("MSH");
        segments[0].Fields.Count.Should().Be(11);
        segments[0].Fields[0].Text.Should().Be(specialChars);
        segments[0].Fields[1].Text.Should().Be("1");
        segments[0].Fields[10].Text.Should().Be("10");

        segments[1].Label.Should().Be("PID");
        segments[1].Fields.Count.Should().Be(3);
        segments[1].Fields[0].Text.Should().Be("a^b");
        segments[1].Fields[1].Text.Should().Be("c^d");
        segments[1].Fields[2].Text.Should().Be("e^f");

        segments[2].Label.Should().Be("PID");
        segments[2].Fields.Count.Should().Be(2);
        segments[2].Fields[0].Text.Should().Be("G&H^I&J");
        segments[2].Fields[1].Text.Should().Be("K&L^M&N");
    }
}
