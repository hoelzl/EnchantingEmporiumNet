using Messages;

namespace MessagesTest;

public class Hl7TestMessageParserSpec : MessageParserSpec
{
    public override int SpecialCharsStartIndex => 3;
    public override int MinSpecialCharsLength => 5;
    public override int MaxSpecialCharsLength => 6;

    // Subfield and repeating separators are swapped with relation to raw messages.
    public override int SubfieldSeparatorIndex => 1;
    public override int RepeatingSeparatorIndex => 2;
}
