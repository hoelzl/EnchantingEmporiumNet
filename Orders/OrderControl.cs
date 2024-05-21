namespace Orders;

public interface IOrderControlAction
{
    Order Execute();
}

public class NewOrderAction(
    Wizard customer,
    Priority priority = Priority.Medium) : IOrderControlAction
{
    public Wizard Customer { get; } = customer;
    public Priority Priority { get; } = priority;

    public Order Execute()
    {
        return new Order {Customer = Customer, Priority = Priority};
    }
}

public class CancelOrderAction(Order order, string reason = "") : IOrderControlAction
{
    public Order Order { get; } = order;
    public string Reason { get; } = reason;

    public Order Execute()
    {
        Order.Status = OrderStatus.Cancelled;
        return Order;
    }
}

public class AddOrderLineAction(Order order, string product, int quantity) : IOrderControlAction
{
    public Order Order { get; } = order;
    public string Product { get; } = product;
    public int Quantity { get; } = quantity;

    public Order Execute()
    {
        Order.Add(Product, Quantity);
        return Order;
    }
}
