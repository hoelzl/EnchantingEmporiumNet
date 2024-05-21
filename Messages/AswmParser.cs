using Orders;

namespace MessageParser;

public class AswmParser : ParserBase<RawMessage, RawSegment>
{
    private readonly Dictionary<string, string> _labelConfig = new()
    {
        {"H", "Header"},
        {"O", "Order"},
        {"L", "OrderLine"}
    };

    private readonly List<(string, int, string)> _fieldConfig =
    [
        ("H", 1, "Sender"),
        ("H", 2, "Receiver"),
        ("O", 0, "Priority"),
        ("O", 1, "Customer"),
        ("L", 0, "Product"),
        ("L", 1, "Quantity"),
    ];

    public OrderBundle ParseMessage(string message)
    {
        using var reader = new StringReader(message);
        return ParseMessage(reader);
    }

    public OrderBundle ParseMessage(TextReader reader)
    {
        var message = ReadText(reader);
        var builder = new OrderBundleBuilder();
        foreach (var segment in message.Segments)
        {
            string segmentName = _labelConfig[segment.Label];
            builder.NewSegment(segmentName);
            foreach (var (label, index, action) in _fieldConfig)
            {
                if (segment.Label == label)
                {
                    string? value = segment.Fields[index].Text;
                    if (value is null)
                        throw new InvalidOperationException($"Field {index} in segment {segment.Label} has no value");
                    builder.SetField(action, value);
                }
            }
        }

        return builder.Build();
    }
}
