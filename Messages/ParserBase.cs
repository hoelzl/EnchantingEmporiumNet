namespace MessageParser;

/** A parser for a message format similar to HL7 or ASTM.
 *
 * We make a number of assumptions about the format:
 * - The first line of the message contains a special character field.
 * - The field starts at index message.SpecialCharsStartIndex.
 * - The field contains between message.MinSpecialCharsLength and message.MaxSpecialCharsLength characters.
 * - The special characters field is followed by a field separator.
 */

public abstract class ParserBase<TMessage, TSegment>
    where TMessage : RawMessage, new() where TSegment : RawSegment, new()
{
    public virtual RawMessage ReadText(TextReader reader)
    {
        string? line = reader.ReadLine();
        var result = CreateRawMessage(line);
        do
        {
            var segment = new TSegment();
            result.Add(segment);
            segment.ParseLine(line!); // Either CreateRawMessage or the while check ensures line is not null.
            line = reader.ReadLine();
        } while (line != null);

        return result;
    }

    private static TMessage CreateRawMessage(string? line)
    {
        var result = new TMessage();
        if (line == null || line.Length < result.MinStartLineLength)
        {
            throw new ArgumentException("Message header too short.", nameof(line));
        }

        char fieldSeparator = line[result.SpecialCharsStartIndex];
        int endOfSpecialChars = line.IndexOf(fieldSeparator, result.SpecialCharsStartIndex + 1);
        if (endOfSpecialChars == -1)
        {
            throw new ArgumentException("End of special char field not found.", nameof(line));
        }

        int numSpecialChars = endOfSpecialChars - result.SpecialCharsStartIndex;
        string specialChars = line.Substring(result.SpecialCharsStartIndex, numSpecialChars);
        result.SpecialChars = specialChars;
        return result;
    }
}
