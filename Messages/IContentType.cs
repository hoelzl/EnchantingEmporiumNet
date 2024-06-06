namespace Messages;

public interface IContentType
{
    bool IsRepeating { get; }
    void Process(string text);
}
