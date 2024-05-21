namespace MessageParser;

public interface IBuilder<out T>
{
    public T Build();
}
