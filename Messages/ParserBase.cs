namespace MessageParser;

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
            segment.ParseFields(line!); // Either CreateRawMessage or the while check ensures line is not null.
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

        string specialChars = line.Substring(result.SpecialCharsStartIndex, result.SpecialCharsLength);
        result.SpecialChars = specialChars;
        return result;
    }
}
