namespace MessagesTest;

using MessageParser;
using FluentAssertions;

public class TestRawField
{
    [Fact]
    public void HasValue_IsTrueIfValueIsNonEmptyString()
    {
        var field = new RawField("value");
        field.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_IsTrueIfValueIsEmptyString()
    {
        var field = new RawField("");
        field.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_IsFalseIfValueIsNull()
    {
        var field = new RawField(null);
        field.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Message_IsNullAfterConstruction()
    {
        var field = new RawField("value");
        field.Message.Should().BeNull();
    }

    [Fact]
    public void HasSubfields_IsFalseIfMessageIsNull()
    {
        var field = new RawField("value^value");
        field.HasSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasSubfields_IsTrueIfValueContainsSubfieldSeparator()
    {
        var field = new RawField("value~value", new RawMessage(){SpecialChars = "|~"});
        field.HasSubfields.Should().BeTrue();
    }
}

public class TestRawSegment
{
    private readonly RawSegment _segment;

    public TestRawSegment()
    {
        var message = new RawMessage();
        message.SpecialChars = "|~";
        _segment = new RawSegment();
        message.Add(_segment);
        _segment.ParseFields("field1|field2|field3");
    }

    [Fact]
    public void Label_IsFirstField()
    {
        _segment.Label.Should().Be("field1");
    }

    [Theory]
    [InlineData(0, "field1")]
    [InlineData(1, "field2")]
    [InlineData(2, "field3")]
    public void Indexer_ReturnsFieldByIndex(int index, string expected)
    {
        _segment[index].Value.Should().Be(expected);
    }

    [Fact]
    public void Fields_ContainsFieldsInOrder()
    {
        var fields = _segment.Fields.Select(field => field.Value).ToArray();

        fields.Should().Equal("field1", "field2", "field3");
    }

    [Fact]
    public void Message_IsNotNullAfterConstruction()
    {
        _segment.Message.Should().NotBeNull();
    }
}

public class TestRawMessage
{
    private readonly RawMessage _message;

    public TestRawMessage()
    {
        _message = new RawMessage
        {
            SpecialChars = "|~"
        };

        var segment1 = new RawSegment();
        _message.Add(segment1);
        segment1.ParseFields("field1|field2|field3|field4~subfield1~subfield2");

        var segment2 = new RawSegment();
        _message.Add(segment2);
        segment2.ParseFields("field5|field6");
    }

    [Fact]
    public void FieldSeparator_IsSetByConstructor()
    {
        _message.FieldSeparator.Should().Be('|');
    }

    [Fact]
    public void SubfieldSeparator_IsSetByConstructor()
    {
        _message.SubfieldSeparator.Should().Be('~');
    }

    [Fact]
    public void Segments_ContainsSegmentsInOrder()
    {
        var segments = _message.Segments.Select(segment => segment.Label).ToArray();

        segments.Should().Equal("field1", "field5");
    }

    [Fact]
    public void Iteration_ReturnsSegmentsInOrder()
    {
        var fields = _message.SelectMany(segment => segment.Fields.Select(field => field.Value)).ToArray();

        fields.Should().Equal("field1", "field2", "field3", "field4~subfield1~subfield2", "field5", "field6");
    }

    [Fact]
    public void Message_IsSetOnSegments()
    {
        foreach (var segment in _message.Segments)
        {
            segment.Message.Should().BeSameAs(_message);
        }
    }

    [Fact]
    public void Message_IsSetOnFields()
    {
        foreach (var segment in _message.Segments)
        {
            foreach (var field in segment.Fields)
            {
                field.Message.Should().BeSameAs(_message);
            }
        }
    }

    [Theory]
    [InlineData(0, "field1")]
    [InlineData(1, "field5")]
    public void Indexer_ReturnsSegmentByIndex(int index, string expected)
    {
        _message[index].Label.Should().Be(expected);
    }

    [Fact]
    public void HasSubfields_IsFalseForFieldWithoutSubfields()
    {
        _message[0][0].HasSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasSubfields_IsTrueForFieldWithSubfields()
    {
        _message[0][3].HasSubfields.Should().BeTrue();
    }

    [Fact]
    public void RemoveItem_SetsMessageToNull()
    {
        var removed = _message[0];
        _message.RemoveAt(0);
        removed.Message.Should().BeNull();
    }

    [Fact]
    public void ClearItems_SetsMessageToNull()
    {
        var segments = _message.Segments.ToArray();
        _message.Clear();
        foreach (var segment in segments)
        {
            segment.Message.Should().BeNull();
            foreach (var field in segment.Fields)
            {
                field.Message.Should().BeNull();
            }
        }
    }
}
