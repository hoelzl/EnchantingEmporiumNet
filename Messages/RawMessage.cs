using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace MessageParser;

public class RawMessage : Collection<RawSegment>
{
    /** The index of the first special character in the first line of the message. */
    public virtual int SpecialCharsStartIndex => 1;

    /** The least number of special characters we expect to find in the first line of the message. */
    public virtual int MinSpecialCharsLength => 2;

    /** The highest number of special characters we expect to find in the first line of the message. */
    public virtual int MaxSpecialCharsLength => 3;

    public virtual bool AreSpecialCharsValid(string specialChars) =>
        specialChars.Length >= MinSpecialCharsLength && specialChars.Length <= MaxSpecialCharsLength;

    /** The minimum length of the first line of the message. */
    public virtual int MinStartLineLength => SpecialCharsStartIndex + MinSpecialCharsLength;


    private string _specialChars = string.Empty;

    /** A string containing all special characters for the message format. Set by the parser. */
    public string SpecialChars
    {
        get => _specialChars.Length > 0
            ? _specialChars
            : throw new InvalidOperationException("SpecialChars not set.");
        set
        {
            if (_specialChars.Length > 0)
            {
                throw new InvalidOperationException("SpecialChars already set.");
            }

            if (AreSpecialCharsValid(value))
            {
                _specialChars = value;
            }
            else
            {
                throw new ArgumentException(
                    $"SpecialChars must be between {MinSpecialCharsLength} and " +
                    $"{MaxSpecialCharsLength} characters long.",
                    nameof(value));
            }
        }
    }

    public virtual char FieldSeparator =>
        SpecialChars[0];

    protected virtual int RepeatingSeparatorIndex => 1;
    protected virtual int SubfieldSeparatorIndex => 2;
    protected virtual int EscapeSeparatorIndex => 3;
    protected virtual int NestedSubfieldSeparatorIndex => 4;
    protected virtual int TruncationCharacterIndex => 5;

    public virtual char RepeatingSeparator =>
        SpecialChars.Length >= RepeatingSeparatorIndex
            ? SpecialChars[RepeatingSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support repeating fields.");

    public virtual char SubfieldSeparator =>
        SpecialChars.Length >= SubfieldSeparatorIndex
            ? SpecialChars[SubfieldSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support subfields.");

    public virtual char EscapeSeparator =>
        SpecialChars.Length >= EscapeSeparatorIndex
            ? SpecialChars[EscapeSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support escape characters.");

    public virtual char NestedSubfieldSeparator =>
        SpecialChars.Length >= NestedSubfieldSeparatorIndex
            ? SpecialChars[NestedSubfieldSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support nested subfields.");

    public virtual char TruncationCharacter =>
        SpecialChars.Length >= TruncationCharacterIndex
            ? SpecialChars[TruncationCharacterIndex]
            : throw new InvalidOperationException("Message format does not support truncation characters.");

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
    private string? _label;

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

    public virtual void ParseLine(string line)
    {
        if (_label != null)
            throw new InvalidOperationException("Segment already contains parse result.");

        if (Message == null)
            throw new InvalidOperationException("Can only parse fields for segments belonging to a message.");


        string[] allFields = line.Split(Message.FieldSeparator);
        _label = allFields[0];
        var fields = allFields.Skip(1);

        var builder = ImmutableList.CreateBuilder<RawField>();
        foreach (string field in fields)
        {
            builder.Add(new RawField(field, Message));
        }

        Fields = builder.ToImmutable();
    }

    public string Label => _label ?? throw new InvalidOperationException("Segment has no label.");
    public RawField this[int index] => Fields[index];

    public virtual string Value(int fieldIndex, int subfieldIndex = 0, int nestedSubfieldIndex = 0)
    {
        return Fields[fieldIndex].Value(subfieldIndex, nestedSubfieldIndex);
    }

    public virtual string[] Values(int fieldIndex, int subfieldIndex = 0, int nestedSubfieldIndex = 0)
    {
        return Fields[fieldIndex].Values(subfieldIndex, nestedSubfieldIndex);
    }
}

public class RawField(string? text, RawMessage? message = null)
{
    public RawMessage? Message { get; internal set; } = message;
    public string? Text { get; } = text;

    // The following methods are not virtual, since the null check is essential for value extraction to be safe.
    // Override Text... to introduce additional checks.
    public bool HasValue => Text != null && TextIsValidValue;
    protected virtual bool TextIsValidValue => true;

    public bool HasSubfields => Message != null && TextHasSubfields;
    protected virtual bool TextHasSubfields => Text?.Contains(Message!.SubfieldSeparator) == true;

    public bool HasNestedSubfields =>
        // Not sure whether we should check TextHasSubfields as well.
        Message != null && TextHasNestedSubfields;

    protected virtual bool TextHasNestedSubfields => Text?.Contains(Message!.NestedSubfieldSeparator) == true;

    public bool IsRepeating => Message != null && TextIsRepeating;
    protected virtual bool TextIsRepeating => Text?.Contains(Message!.RepeatingSeparator) == true;

    public virtual string Value(int subfieldIndex = 0, int nestedSubfieldIndex = 0)
    {
        if (!HasValue) throw new InvalidOperationException("Field has no value.");
        if (IsRepeating)
            throw new InvalidOperationException("Cannot extract single value from repeating field.");
        return ExtractValue(Text!, subfieldIndex, nestedSubfieldIndex);
    }

    public virtual string[] Values(int subfieldIndex = 0, int nestedSubfieldIndex = 0)
    {
        if (!HasValue) throw new InvalidOperationException("Field has no value.");
        return Text!
            .Split(Message!.RepeatingSeparator)
            .Select(t => ExtractValue(t, subfieldIndex, nestedSubfieldIndex))
            .ToArray();
    }

    private string ExtractValue(string text, int subfieldIndex, int nestedSubfieldIndex)
    {
        if (HasSubfields)
        {
            string[] subfields = text.Split(Message!.SubfieldSeparator);
            string subfield = subfields.ElementAtOrDefault(subfieldIndex) ??
                              throw new ArgumentException("Subfield index out of range.", nameof(subfieldIndex));
            if (HasNestedSubfields)
            {
                string[] nestedSubfields = subfield.Split(Message!.NestedSubfieldSeparator);
                return nestedSubfields.ElementAtOrDefault(nestedSubfieldIndex) ??
                       throw new ArgumentException("Nested subfield index out of range.", nameof(nestedSubfieldIndex));
            }
            else
            {
                return subfield;
            }
        }
        else
        {
            return text;
        }
    }
}
