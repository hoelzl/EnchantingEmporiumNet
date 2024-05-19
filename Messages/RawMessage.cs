using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace MessageParser;

public class RawMessage : Collection<RawSegment>
{
    /** The index of the first special character in the first line of the message. */
    public virtual int SpecialCharsStartIndex => 3;

    /** The number of special characters we expect to find in the first line of the message. */
    public virtual int SpecialCharsLength => 2;

    /** The minimum length of the first line of the message. */
    public virtual int MinStartLineLength => SpecialCharsStartIndex + SpecialCharsLength;


    private string _specialChars = string.Empty;

    /** A string containing all special characters for the message format. Set by the parser. */
    public string SpecialChars
    {
        get => _specialChars.Length == SpecialCharsLength
            ? _specialChars
            : throw new InvalidOperationException("SpecialChars not set.");
        set
        {
            if (_specialChars.Length > 0)
            {
                throw new InvalidOperationException("SpecialChars already set.");
            }

            if (value.Length == SpecialCharsLength)
            {
                _specialChars = value;
            }
            else
            {
                throw new ArgumentException($"SpecialChars must be exactly {SpecialCharsLength} characters long.",
                    nameof(value));
            }
        }
    }

    public virtual char FieldSeparator =>
        SpecialChars[0];

    public virtual char SubfieldSeparator =>
        SpecialChars[1];

    public IEnumerable<RawSegment> Segments => Items;

    protected override void InsertItem(int index, RawSegment item)
    {
        base.InsertItem(index, item);
        item.Message = this;
    }

    protected override void SetItem(int index, RawSegment item)
    {
        base.SetItem(index, item);
        item.Message = this;
    }

    protected override void RemoveItem(int index)
    {
        Items[index].Message = null;
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        foreach (var item in Items)
        {
            item.Message = null;
        }

        base.ClearItems();
    }
}


// I would prefer to pass the message to the constructor of the segment, but this is difficult to do if we want
// to have a parser that is generic in the type of message and segment it understands, since we can only constrain
// the segment to have a parameterless constructor.
// Maybe this can be fixed with virtual static methods or something, so that we can have a factory method.
//
// Right now we have an ugly temporal coupling where you have to
// - create the segment
// - add it to the message
// - parse the fields
// in that order.
public class RawSegment
{
    private RawMessage? _message;

    public RawMessage? Message
    {
        get => _message;
        internal set
        {
            if (_message != null && value != null)
            {
                throw new InvalidOperationException("Message already set.");
            }

            _message = value;
            foreach (var field in Fields)
            {
                field.Message = value;
            }
        }
    }

    public ImmutableList<RawField> Fields { get; private set; } = ImmutableList<RawField>.Empty;

    public virtual void ParseFields(string line)
    {
        if (Message == null)
        {
            throw new InvalidOperationException("Can only parse fields for segments belonging to a message.");
        }

        string[] fields = line.Split(Message.FieldSeparator);
        var builder = ImmutableList.CreateBuilder<RawField>();
        foreach (string field in fields)
        {
            builder.Add(new RawField(field, Message));
        }

        Fields = builder.ToImmutable();
    }

    public string Label => Fields[0].Value ?? throw new InvalidOperationException("Segment has no label.");
    public RawField this[int index] => Fields[index];
}

public class RawField(string? value, RawMessage? message = null)
{
    public RawMessage? Message { get; internal set; } = message;
    public string? Value { get; } = value;

    public bool HasValue => Value != null;
    public bool HasSubfields => Message != null && Value?.Contains(Message.SubfieldSeparator) == true;
}
