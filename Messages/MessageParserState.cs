namespace Messages;

public class MessageParserState
{
    public MessageParserSpec Spec { get; }
    public string SpecialChars { get; }

    /** A string containing all special characters for the message format. Set by the parser. */
    public MessageParserState(MessageParserSpec spec, string header)
    {
        Spec = spec;
        SpecialChars = Spec.ExtractSpecialCharsFromHeader(header);
    }

    public virtual char ComponentSeparator =>
        SpecialChars[0];

    public virtual char RepeatingSeparator =>
        HasRepeatingSeparator
            ? SpecialChars[Spec.RepeatingSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support repeating fields.");

    public bool HasRepeatingSeparator => SpecialChars.Length > Spec.RepeatingSeparatorIndex;

    public virtual char SubcomponentSeparator =>
        HasSubcomponentSeparator
            ? SpecialChars[Spec.SubfieldSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support subfields.");

    public bool HasSubcomponentSeparator => SpecialChars.Length > Spec.SubfieldSeparatorIndex;

    public virtual char EscapeChar =>
        HasEscapeChar
            ? SpecialChars[Spec.EscapeSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support escape characters.");

    public bool HasEscapeChar => SpecialChars.Length > Spec.EscapeSeparatorIndex;

    public virtual char NestedSubcomponentSeparator =>
        HasNestedSubcomponentSeparator
            ? SpecialChars[Spec.NestedSubfieldSeparatorIndex]
            : throw new InvalidOperationException("Message format does not support nested subfields.");

    public bool HasNestedSubcomponentSeparator => SpecialChars.Length > Spec.NestedSubfieldSeparatorIndex;

    public virtual char TruncationCharacter =>
        HasTruncationCharacter
            ? SpecialChars[Spec.TruncationCharacterIndex]
            : throw new InvalidOperationException("Message format does not support truncation characters.");

    public bool HasTruncationCharacter => SpecialChars.Length > Spec.TruncationCharacterIndex;
}
