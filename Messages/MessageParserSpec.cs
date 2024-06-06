namespace Messages;

public class MessageParserSpec
{

    public static MessageParserSpec Default { get; } = new MessageParserSpec();

    /** The index of the first special character in the first line of the message. */
    public virtual int SpecialCharsStartIndex => 1;

    /** The least number of special characters we expect to find in the first line of the message. */
    public virtual int MinSpecialCharsLength => 2;

    /** The highest number of special characters we expect to find in the first line of the message. */
    public virtual int MaxSpecialCharsLength => 3;

    public virtual int RepeatingSeparatorIndex => 1;
    public virtual int SubfieldSeparatorIndex => 2;
    public virtual int EscapeSeparatorIndex => 3;
    public virtual int NestedSubfieldSeparatorIndex => 4;
    public virtual int TruncationCharacterIndex => 5;

    /** Does the spec have a fixed number of special characters? */
    public virtual bool HasFixedSpecialChars => MinSpecialCharsLength == MaxSpecialCharsLength;

    public virtual bool AreSpecialCharsValid(string specialChars) =>
        specialChars.Length >= MinSpecialCharsLength && specialChars.Length <= MaxSpecialCharsLength;

    /** The minimum length of the first line of the message. */
    public virtual int MinStartLineLength => SpecialCharsStartIndex + MinSpecialCharsLength;

    /** Extract the special characters from the first line of the message. */
    public string ExtractSpecialCharsFromHeader(string? headerLine)
    {
        if (headerLine == null || headerLine.Length < MinStartLineLength)
        {
            throw new ArgumentException("Message header too short.", nameof(headerLine));
        }

        char fieldSeparator = headerLine[SpecialCharsStartIndex];
        int endOfSpecialChars = headerLine.IndexOf(fieldSeparator, SpecialCharsStartIndex + 1);
        if (endOfSpecialChars == -1)
        {
            throw new ArgumentException("End of special char field not found.", nameof(headerLine));
        }

        int numSpecialChars = endOfSpecialChars - SpecialCharsStartIndex;
        string specialChars = headerLine.Substring(SpecialCharsStartIndex, numSpecialChars);

        if (!AreSpecialCharsValid(specialChars))
        {
            throw new ArgumentException(
                HasFixedSpecialChars
                    ? $"Special chars must be exactly {MinSpecialCharsLength} characters long."
                    : $"Special chars must be between {MinSpecialCharsLength} and " +
                      $"{MaxSpecialCharsLength} characters long.",
                nameof(specialChars));
        }
        return specialChars;
    }
}
