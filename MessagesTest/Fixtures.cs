using MessageParser;

namespace MessagesTest;

public class Hl7TestMessage : RawMessage
{
    public override int SpecialCharsStartIndex => 3;
    public override int MinSpecialCharsLength => 5;
    public override int MaxSpecialCharsLength => 6;

    // Subfield and repeating separators are swapped with relation to raw messages.
    protected override int SubfieldSeparatorIndex => 1;
    protected override int RepeatingSeparatorIndex => 2;
}
