using System.Diagnostics;

namespace Messages;

[Flags]
public enum MessageParserFlags
{
    IsHeaderLine = 1,
}

/** A parser for a message format similar to HL7 or ASTM.
 *
 * We make a number of assumptions about the format:
 * - The first line of the message contains a special character field.
 * - The field starts at index spec.SpecialCharsStartIndex.
 * - The field contains between spec.MinSpecialCharsLength and spec.MaxSpecialCharsLength characters.
 * - The special characters field is followed by a field separator.
 */
public class MessageParser(MessageParserSpec spec)
{
    private MessageParserSpec Spec { get; } = spec;

    public virtual ParsedMessage ReadText(TextReader reader)
    {
        string? line = reader.ReadLine();
        if (line == null)
        {
            throw new ArgumentException("Message header missing.", nameof(reader));
        }

        var state = new MessageParserState(Spec, line);
        var result = new ParsedMessage();
        var flags = MessageParserFlags.IsHeaderLine;

        do
        {
            Record record = ParseRecord(state, line);
            result.Records.Add(record);
            line = reader.ReadLine();
        } while (line != null);

        return result;
    }

    public static Record ParseRecord(MessageParserState parserState, string line)
    {
        Debug.WriteLine($"ParseRecord({line})");
        string[] splitLine = line.Split(parserState.ComponentSeparator);
        string label = splitLine[0];

        var components =
            splitLine
                .Skip(1)
                .Select(componentString => ParseComponent(parserState, componentString))
                .ToList();

        Debug.WriteLine(
            $"ParseRecord: {label}, {string.Join(", ", components.Select(c => "\"" + c.Value() + "\""))}");
        return new Record(label, components);
    }

    public static Component ParseComponent(MessageParserState parserSpec, string componentString)
    {
        Debug.WriteLine($"ParseComponent({componentString})");
        var repetitions =
            parserSpec.HasRepeatingSeparator
                ? componentString
                    .Split(parserSpec.RepeatingSeparator)
                    .Select(rep => ParseRepetition(parserSpec, rep))
                    .ToList()
                : [ParseRepetition(parserSpec, componentString)];

        Debug.WriteLine(
            $"ParseComponent: {string.Join(", ", repetitions.Select(r => "\"" + r.Value() + "\""))}");
        return new Component(repetitions);
    }

    public static Repetition ParseRepetition(MessageParserState parserSpec, string repetitionString)
    {
        Debug.WriteLine($"ParseRepetition({repetitionString})");
        var subcomponents =
            parserSpec.HasSubcomponentSeparator
                ? repetitionString
                    .Split(parserSpec.SubcomponentSeparator)
                    .Select(sc => ParseSubcomponent(parserSpec, sc))
                    .ToList()
                : [ParseSubcomponent(parserSpec, repetitionString)];

        Debug.WriteLine(
            $"ParseRepetition: {string.Join(", ", subcomponents.Select(sc => "\"" + sc.Value() + "\""))}");
        return new Repetition(subcomponents);
    }

    public static Subcomponent ParseSubcomponent(MessageParserState parserSpec, string subcomponentString)
    {
        var nestedSubcomponents =
            parserSpec.HasNestedSubcomponentSeparator
                ? subcomponentString
                    .Split(parserSpec.NestedSubcomponentSeparator)
                    .Select(nsc => ParseNestedSubcomponent(parserSpec, nsc))
                : new NestedSubcomponent[] {ParseNestedSubcomponent(parserSpec, subcomponentString)};
        return new Subcomponent(nestedSubcomponents);
    }

    public static NestedSubcomponent ParseNestedSubcomponent(MessageParserState parserSpec, string? text)
    {
        return new NestedSubcomponent(text);
    }
}
