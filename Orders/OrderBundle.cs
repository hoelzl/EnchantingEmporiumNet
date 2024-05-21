namespace Orders;

public class OrderBundle
{
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public List<Order> Orders { get; init; } = new();
}
