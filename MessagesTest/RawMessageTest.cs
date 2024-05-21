namespace MessagesTest;

using MessageParser;
using FluentAssertions;

public class TestRawField
{
    private readonly RawMessage _message = new Hl7TestMessage() {SpecialChars = "|^~\\&"};

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
    public void Message_IsSetByConstructor()
    {
        var field = new RawField("value", _message);
        field.Message.Should().BeSameAs(_message);
    }

    [Fact]
    public void HasSubfields_IsFalseIfMessageIsNull()
    {
        var field = new RawField("value^value");
        field.HasSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasSubfields_IsFalseIfValueDoesNotContainSubfieldSeparator()
    {
        var field = new RawField("value", _message);
        field.HasSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasSubfields_IsTrueIfValueContainsSubfieldSeparator()
    {
        var field = new RawField("value^value", _message);
        field.HasSubfields.Should().BeTrue();
    }

    [Fact]
    public void HasNestedSubfields_IsFalseIfMessageIsNull()
    {
        var field = new RawField("value&1^value&2");
        field.HasNestedSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasNestedSubfields_IsFalseIfValueDoesNotContainNestedSubfieldSeparator()
    {
        var field = new RawField("value_1^value_2", _message);
        field.HasNestedSubfields.Should().BeFalse();
    }

    [Fact]
    public void HasNestedSubfields_IsTrueIfValueContainsNestedSubfieldSeparator()
    {
        var field = new RawField("value&1^value&2", _message);
        field.HasNestedSubfields.Should().BeTrue();
    }

    [Fact]
    public void IsRepeating_IsFalseIfMessageIsNull()
    {
        var field = new RawField("value1~value2");
        field.IsRepeating.Should().BeFalse();
    }

    [Fact]
    public void IsRepeating_IsFalseIfValueDoesNotContainRepeatingSeparator()
    {
        var field = new RawField("value1+value2", _message);
        field.IsRepeating.Should().BeFalse();
    }

    [Fact]
    public void IsRepeating_IsTrueIfValueContainsRepeatingSeparator()
    {
        var field = new RawField("value1~value2", _message);
        field.IsRepeating.Should().BeTrue();
    }

    [Fact]
    public void Value_ThrowsInvalidOperationExceptionIfValueIsNull()
    {
        var field = new RawField(null);
        field.Invoking(f => f.Value()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Value_ReturnsTextForSimpleValue()
    {
        var field = new RawField("value", _message);
        field.Value().Should().Be("value");
    }

    [Fact]
    public void Value_ReturnsSubfield0ByDefault()
    {
        var field = new RawField("value1^value2", _message);
        field.Value().Should().Be("value1");
    }

    [Theory]
    [InlineData(0, "value1")]
    [InlineData(1, "value2")]
    public void Value_ReturnsSubfieldForGivenIndex(int index, string expected)
    {
        var field = new RawField("value1^value2", _message);
        field.Value(index).Should().Be(expected);
    }

    [Fact]
    public void Value_ThrowsArgumentExceptionIfValueIfSubfieldIndexIsOutOfRange()
    {
        var field = new RawField("value1^value2", _message);
        field.Invoking(f => f.Value(2)).Should().Throw<ArgumentException>();
    }


    [Fact]
    public void Value_ReturnsNestedSubfield0ByDefault()
    {
        var field = new RawField("value1&1^value2&2", _message);
        field.Value().Should().Be("value1");
    }

    [Theory]
    [InlineData(0, 0, "value1")]
    [InlineData(0, 1, "1")]
    [InlineData(1, 0, "value2")]
    [InlineData(1, 1, "2")]
    public void Value_ReturnsNestedSubfieldForGivenIndex(int index, int subIndex, string expected)
    {
        var field = new RawField("value1&1^value2&2", _message);
        field.Value(index, subIndex).Should().Be(expected);
    }

    [Fact]
    public void Value_ThrowsArgumentExceptionIfNestedSubfieldIndexIsOutOfRange()
    {
        var field = new RawField("value1&1^value2&2", _message);
        field.Invoking(f => f.Value(0, 2)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Values_ReturnsArrayWithTextForSimpleValue()
    {
        var field = new RawField("value", _message);
        field.Values().Should().Equal(["value"]);
    }

    [Fact]
    public void Values_ReturnsArrayWithTextsForRepeatedValue()
    {
        var field = new RawField("value1~value2", _message);
        field.Values().Should().Equal(["value1", "value2"]);
    }

    [Theory]
#pragma warning disable CA1861
    [InlineData("v1^v2", 0, new[] {"v1"})]
    [InlineData("v1^v2", 1, new[] {"v2"})]
    [InlineData("v1^v2~w1^w2", 0, new[] {"v1", "w1"})]
    [InlineData("v1^v2~w1^w2", 1, new[] {"v2", "w2"})]
#pragma warning restore CA1861
    public void Values_ReturnsArrayWithSubfieldValuesForGivenIndex(string value, int index, string[] expected)
    {
        var field = new RawField(value, _message);
        field.Values(index).Should().Equal(expected);
    }

    [Theory]
#pragma warning disable CA1861
    [InlineData("v1&1^v2&2", 0, 0, new[] {"v1"})]
    [InlineData("v1&1^v2&2", 0, 1, new[] {"1"})]
    [InlineData("v1&1^v2&2", 1, 0, new[] {"v2"})]
    [InlineData("v1&1^v2&2", 1, 1, new[] {"2"})]
    [InlineData("v1&1^v2&2~w1&1^w2&2", 0, 0, new[] {"v1", "w1"})]
    [InlineData("v1&1^v2&2~w1&1^w2&2", 0, 1, new[] {"1", "1"})]
    [InlineData("v1&1^v2&2~w1&1^w2&2", 1, 0, new[] {"v2", "w2"})]
    [InlineData("v1&1^v2&2~w1&1^w2&2", 1, 1, new[] {"2", "2"})]
#pragma warning restore CA1861
    public void Values_ReturnsArrayWithNestedSubfieldValuesForGivenIndex(string text, int index, int subIndex,
        string[] expected)
    {
        var field = new RawField(text, _message);
        field.Values(index, subIndex).Should().Equal(expected);
    }

    [Theory]
    [InlineData("v1&1^v2&2", 0, 2)]
    [InlineData("v1&1^v2&2", 2, 0)]
    public void Values_ThrowsArgumentExceptionIfIndexIsOutOfRange(string text, int index, int subIndex)
    {
        var field = new RawField(text, _message);
        field.Invoking(f => f.Values(index, subIndex)).Should().Throw<ArgumentException>();
    }
}

public class TestRawSegment
{
    private readonly RawSegment _segment;

    public TestRawSegment()
    {
        // ReSharper disable once CollectionNeverQueried.Local
        var message = new RawMessage
        {
            SpecialChars = "|~"
        };
        _segment = new RawSegment();
        message.Add(_segment);
        _segment.ParseLine("label|field1|field2|field3");
    }

    [Fact]
    public void Label_IsFirstField()
    {
        _segment.Label.Should().Be("label");
    }

    [Theory]
    [InlineData(0, "field1")]
    [InlineData(1, "field2")]
    [InlineData(2, "field3")]
    public void Indexer_ReturnsFieldByIndex(int index, string expected)
    {
        _segment[index].Text.Should().Be(expected);
    }

    [Fact]
    public void Fields_ContainsFieldsInOrder()
    {
        var fields = _segment.Fields.Select(field => field.Text).ToArray();

        fields.Should().Equal("field1", "field2", "field3");
    }

    [Fact]
    public void Message_IsNotNullAfterConstruction()
    {
        _segment.Message.Should().NotBeNull();
    }
}

public class RawMessageTest
{
    private readonly RawMessage _message;

    public RawMessageTest()
    {
        _message = new RawMessage
        {
            SpecialChars = "|\\^"
        };

        var segment1 = new RawSegment();
        _message.Add(segment1);
        segment1.ParseLine("label1|field1|field2|field3|field4^subfield1^subfield2");

        var segment2 = new RawSegment();
        _message.Add(segment2);
        segment2.ParseLine("label2|field5|field6");
    }

    [Fact]
    public void FieldSeparator_IsSetByConstructor()
    {
        _message.FieldSeparator.Should().Be('|');
    }

    [Fact]
    public void SubfieldSeparator_IsSetByConstructor()
    {
        _message.SubfieldSeparator.Should().Be('^');
    }

    [Fact]
    public void Segments_ContainsSegmentsInOrder()
    {
        var segments = _message.Segments.Select(segment => segment.Label).ToArray();

        segments.Should().Equal("label1", "label2");
    }

    [Fact]
    public void Iteration_ReturnsFieldsInOrder()
    {
        string?[] fields = _message.SelectMany(segment => segment.Fields.Select(field => field.Text)).ToArray();

        fields.Should().Equal("field1", "field2", "field3", "field4^subfield1^subfield2", "field5", "field6");
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
    [InlineData(0, "label1")]
    [InlineData(1, "label2")]
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
