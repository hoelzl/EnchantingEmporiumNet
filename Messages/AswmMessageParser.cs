using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Orders;

namespace Messages;

// ReSharper disable once ClassNeverInstantiated.Global
public class FieldLabelConfig
{
    public string Label { get; set; } = "";
    public string Action { get; set; } = "";
}

public class FieldActionConfig
{
    [Index(0)] public string Label { get; set; } = "";
    [Index(1)] public int FieldIndex { get; set; } = 0;
    [Index(2)] public int SubfieldIndex { get; set; } = 0;
    [Index(3)] public string Action { get; set; } = "";

    public void Deconstruct(out string label, out int index, out string action)
    {
        label = Label;
        index = FieldIndex;
        action = Action;
    }

    public void Deconstruct(out string label, out int index, out int subfieldIndex, out string action)
    {
        label = Label;
        index = FieldIndex;
        subfieldIndex = SubfieldIndex;
        action = Action;
    }
}

public class AswmMessageParser : MessageParser
{
    public AswmMessageParser(MessageParserSpec spec, TextReader labelConfigReader, TextReader fieldConfigReader) :
        base(spec)
    {
        using (var csv = new CsvReader(labelConfigReader, CultureInfo.InvariantCulture))
        {
            _labelConfig = csv.GetRecords<FieldLabelConfig>().ToDictionary(x => x.Label, x => x.Action);
        }

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {HasHeaderRecord = false};
        using (var csv = new CsvReader(fieldConfigReader, csvConfig))
        {
            _fieldConfig = csv.GetRecords<FieldActionConfig>().ToList();
        }
    }

    private readonly Dictionary<string, string> _labelConfig;
    private readonly List<FieldActionConfig> _fieldConfig;

    public OrderBundle ParseMessage(string message)
    {
        using var reader = new StringReader(message);
        return ParseMessage(reader);
    }

    public OrderBundle ParseMessage(TextReader reader)
    {
        var message = ReadText(reader);
        var builder = new OrderBundleBuilder();
        foreach (var record in message.Records)
        {
            string segmentName = _labelConfig[record.Label];
            builder.NewSegment(segmentName);
            foreach (var (label, index, action) in _fieldConfig)
            {
                if (record.Label == label)
                {
                    string? value = record.Value(index);
                    if (value is null)
                        throw new InvalidOperationException($"Field {index} in segment {record.Label} has no value");
                    builder.SetField(action, value);
                }
            }
        }

        return builder.Build();
    }
}
