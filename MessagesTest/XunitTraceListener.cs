using System.Diagnostics;
using Xunit.Abstractions;

public class XunitTraceListener : TraceListener
{
    private readonly ITestOutputHelper _output;

    public XunitTraceListener(ITestOutputHelper output)
    {
        _output = output;
    }

    public override void Write(string? message)
    {
        _output.WriteLine(message);
    }

    public override void WriteLine(string? message)
    {
        _output.WriteLine(message);
    }
}
