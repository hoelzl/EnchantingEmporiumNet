using Messages;
using FluentAssertions;
using Record = Messages.Record;

namespace MessagesTest;

public class TestComponent
{
    private readonly Component _componentWithSubcomponents = new(new List<Repetition>
        {new Repetition(new List<string> {"sc1", "sc2"})});

    private readonly Component _componentWithNestedSubcomponents = new(new List<Repetition>
    {
        new Repetition(new List<Subcomponent>
        {
            new Subcomponent(new List<string> {"sc1.nsc1", "sc1.nsc2"}),
            new Subcomponent(new List<string> {"sc2.nsc1", "sc2.nsc2"})
        })
    });

    private readonly Component _repeatedComponentWithSubcomponents = new(new List<Repetition>
    {
        new Repetition(new List<string> {"rep1.sc1", "rep1.sc2"}),
        new Repetition(new List<string> {"rep2.sc1", "rep2.sc2"})
    });

    [Fact]
    public void HasValue_IsTrueIfValueIsNonEmptyString()
    {
        var component = new Component("value");
        component.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_IsTrueIfValueIsEmptyString()
    {
        var field = new Component("");
        field.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_IsFalseIfValueIsNull()
    {
        var field = new Component();
        field.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Value_ThrowsInvalidOperationExceptionIfValueIsNull()
    {
        var field = new Component();
        field.Invoking(f => f.Value()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Value_ReturnsTextForSimpleValue()
    {
        var field = new Component("value");
        field.Value().Should().Be("value");
    }

    [Fact]
    public void Value_ReturnsSubcomponent0ByDefault()
    {
        _componentWithSubcomponents.Value().Should().Be("sc1");
    }

    [Theory]
    [InlineData(0, "sc1")]
    [InlineData(1, "sc2")]
    public void Value_ReturnsSubcomponentForGivenIndex(int index, string expected)
    {
        _componentWithSubcomponents.Value(index).Should().Be(expected);
    }

    [Fact]
    public void Value_ThrowsArgumentExceptionIfValueIfSubcomponentIndexIsOutOfRange()
    {
        _componentWithSubcomponents.Invoking(c => c.Value(2)).Should().Throw<ArgumentException>();
    }


    [Fact]
    public void Value_ReturnsNestedSubcomponent0ByDefault()
    {
        _componentWithNestedSubcomponents.Value().Should().Be("sc1.nsc1");
    }

    [Theory]
    [InlineData(0, 0, "sc1.nsc1")]
    [InlineData(0, 1, "sc1.nsc2")]
    [InlineData(1, 0, "sc2.nsc1")]
    [InlineData(1, 1, "sc2.nsc2")]
    public void Value_ReturnsNestedSubcomponentForGivenIndex(int index, int subIndex, string expected)
    {
        _componentWithNestedSubcomponents.Value(index, subIndex).Should().Be(expected);
    }

    [Fact]
    public void Value_ThrowsArgumentExceptionIfNestedSubcomponentIndexIsOutOfRange()
    {
        _componentWithNestedSubcomponents.Invoking(c => c.Value(0, 2)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Values_ReturnsArrayWithTextForSimpleValue()
    {
        var field = new Component("value");
        field.RepeatingValues().Should().Equal(["value"]);
    }

    [Fact]
    public void Values_ReturnsArrayWithTextsForRepeatedValue()
    {
        _repeatedComponentWithSubcomponents.RepeatingValues().Should().Equal(["rep1.sc1", "rep2.sc1"]);
    }
}

public class TestRecord
{
    private readonly Record _record;

    public TestRecord()
    {
        // ReSharper disable once CollectionNeverQueried.Local
        var message = new ParsedMessage();
        _record = new Record("label", ["field1", "field2", "field3"]);
        message.Records.Add(_record);
    }

    [Fact]
    public void Label_IsFirstField()
    {
        _record.Label.Should().Be("label");
    }

    [Theory]
    [InlineData(0, "field1")]
    [InlineData(1, "field2")]
    [InlineData(2, "field3")]
    public void Indexer_ReturnsFieldByIndex(int index, string expected)
    {
        _record[index].Value().Should().Be(expected);
    }

    [Fact]
    public void Fields_ContainsFieldsInOrder()
    {
        var fields = _record.Components.Select(field => field.Value()).ToArray();

        fields.Should().Equal("field1", "field2", "field3");
    }
}

public class ParsedMessageTest
{
    private readonly ParsedMessage _message;

    public ParsedMessageTest()
    {
        var record1 = new Record("label1", ["field1", "field2", "field3", "field4^subfield1^subfield2"]);
        var record2 = new Record("label2", ["field5", "field6"]);
        _message = new ParsedMessage([record1, record2]);
    }

    [Fact]
    public void Segments_ContainsSegmentsInOrder()
    {
        var records = _message.Records.Select(segment => segment.Label).ToArray();

        records.Should().Equal("label1", "label2");
    }

    [Fact]
    public void Iteration_ReturnsFieldsInOrder()
    {
        var components =
            _message
                .Records
                .SelectMany(record => record.Components.Select(c => c.Value()));

        components.Should().Equal("field1", "field2", "field3", "field4^subfield1^subfield2", "field5", "field6");
    }

    [Theory]
    [InlineData(0, "label1")]
    [InlineData(1, "label2")]
    public void Indexer_ReturnsSegmentByIndex(int index, string expected)
    {
        _message[index].Label.Should().Be(expected);
    }
}
