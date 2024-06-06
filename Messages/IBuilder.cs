namespace Messages;

public interface IBuilder<out T>
{
    public T Build();
}
