namespace Messages;

public class ParsedMessage(IEnumerable<Record>? records = null)
{
    public List<Record> Records { get; } = records?.ToList() ?? [];
    public Record this[int index] => Records[index];
}

public class Record(string label, IEnumerable<Component>? components)
{
    public Record(string label, IEnumerable<string?> componentTexts) :
        this(label, componentTexts.Select(t => new Component(t)))
    {
    }

    public string Label { get; set; } = label;
    public List<Component> Components { get; set; } = components?.ToList() ?? [];

    public Component this[int index] => Components[index];

    public virtual string Value(int componentIndex, int subcomponentIndex = 0, int nestedSubcomponentIndex = 0)
    {
        return Components[componentIndex].Value(subcomponentIndex, nestedSubcomponentIndex);
    }

    public virtual List<string> RepeatingValues(int componentIndex, int subcomponentIndex = 0,
        int nestedSubcomponentIndex = 0)
    {
        return Components[componentIndex].RepeatingValues(subcomponentIndex, nestedSubcomponentIndex);
    }
}

public class Component
{
    public Component(IEnumerable<Repetition> repetitions)
    {
        Repetitions = repetitions.ToList();
    }

    public Component(IEnumerable<string?> repetitionTexts)
    {
        Repetitions = repetitionTexts.Select(t => new Repetition(t)).ToList();
    }

    public Component(string? repetitionText = null)
    {
        Repetitions = [new Repetition(repetitionText)];
    }

    public List<Repetition> Repetitions { get; set; }

    public bool HasValue => Repetitions.Any(r => r.HasValue);
    public bool IsRepeating { get; set; } = false;

    public virtual string Value(int subcomponentIndex = 0, int nestedSubcomponentIndex = 0)
    {
        AssertHasValue();
        if (IsRepeating) throw new InvalidOperationException("Cannot get single value for repeating content.");
        return Repetitions[0].Value(subcomponentIndex, nestedSubcomponentIndex);
    }

    public virtual List<string> RepeatingValues(int subcomponentIndex = 0, int nestedSubcomponentIndex = 0)
    {
        AssertHasValue();
        return Repetitions.Select(r => r.Value(subcomponentIndex, nestedSubcomponentIndex)).ToList();
    }

    protected virtual void AssertHasValue()
    {
        if (!HasValue) throw new InvalidOperationException("Field has no value.");
    }
}

public class Repetition
{
    public Repetition(IEnumerable<Subcomponent> subcomponents)
    {
        Subcomponents = subcomponents.ToList();
    }

    public Repetition(IEnumerable<string?> subcomponentTexts)
    {
        Subcomponents = subcomponentTexts.Select(t => new Subcomponent(t)).ToList();
    }

    public Repetition(string? subcomponentText = null)
    {
        Subcomponents = [new Subcomponent(subcomponentText)];
    }

    public List<Subcomponent> Subcomponents { get; set; }
    public Subcomponent this[int index] => Subcomponents[index];

    public bool HasValue => Subcomponents.Any(s => s.HasValue);

    public string Value(int subcomponentIndex = 0, int nestedSubcomponentIndex = 0)
    {
        return Subcomponents[subcomponentIndex].Value(nestedSubcomponentIndex);
    }

    public List<string> RepeatingValues(int subcomponentIndex = 0, int nestedSubcomponentIndex = 0)
    {
        return Subcomponents.Select(sc => sc.Value(nestedSubcomponentIndex)).ToList();
    }
}

public class Subcomponent
{
    public Subcomponent(IEnumerable<NestedSubcomponent> nestedSubcomponents)
    {
        NestedSubcomponents = nestedSubcomponents.ToList();
    }

    public Subcomponent(IEnumerable<string?> nestedSubcomponentTexts)
    {
        NestedSubcomponents = nestedSubcomponentTexts.Select(t => new NestedSubcomponent(t)).ToList();
    }

    public Subcomponent(string? nestedSubcomponentText = null)
    {
        NestedSubcomponents = new List<NestedSubcomponent> {new NestedSubcomponent(nestedSubcomponentText)};
    }

    public List<NestedSubcomponent> NestedSubcomponents { get; set; }
    public NestedSubcomponent this[int index] => NestedSubcomponents[index];

    public bool HasValue => NestedSubcomponents.Any(n => n.HasValue);

    public string Value(int nestedSubcomponentIndex = 0)
    {
        return this[nestedSubcomponentIndex].Text;
    }
}

public class NestedSubcomponent(string? text = null)
{
    private string? _text = text;
    public bool HasValue => _text != null;

    public string Text
    {
        get => _text ?? throw new InvalidOperationException("Subcomponent has no value.");
        set => _text = value;
    }
}
