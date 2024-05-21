using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Orders;

public enum Priority
{
    Low,
    Medium,
    High
}

public class Order : Collection<OrderLine>, IEquatable<Order>
{
    private static readonly ConcurrentDictionary<Ulid, Order> Orders = new();

    public static Order GetOrder(Ulid id) => Orders[id];

    public static bool TryGetOrder(Ulid id, [NotNullWhen(true)] out Order? order)
    {
        return Orders.TryGetValue(id, out order);
    }

    public Order()
    {
        Orders[Id] = this;
    }

    public Ulid Id { get; } = Ulid.NewUlid();
    public Wizard? Customer { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public IList<OrderLine> Lines => Items;

    public override string ToString() => $"Order {Id}";


    public override bool Equals(object? obj) => obj is Order order && order.Id == Id;
    public bool Equals(Order? other) => other?.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Order? left, Order? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Order? left, Order? right) => !(left == right);

    public OrderLine Add(string product, int quantity)
    {
        var line = new OrderLine(this) {Product = product, Quantity = quantity};
        Add(line);
        return line;
    }

    protected override void ClearItems()
    {
        foreach (OrderLine line in Items)
        {
            line.Order = null;
        }

        base.ClearItems();
    }

    protected override void InsertItem(int index, OrderLine item)
    {
        SetItemOrder(item);
        base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        OrderLine removedItem = Items[index];
        base.RemoveItem(index);
        removedItem.Order = null;
    }

    protected override void SetItem(int index, OrderLine item)
    {
        OrderLine oldItem = Items[index];
        SetItemOrder(item);
        base.SetItem(index, item);
        oldItem.Order = null;
    }

    private void SetItemOrder(OrderLine item)
    {
        if (item.Order == null)
        {
            item.Order = this;
        }
        else if (item.Order != this)
        {
            throw new ArgumentException(
                "OrderLine does not belong to this order.", nameof(item));
        }
    }
}

public enum OrderStatus
{
    New,
    InProgress,
    Completed,
    Cancelled
}

public class OrderLine
{
    public Order? Order { get; set; }
    public string? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; } = 0m;

    public OrderLine(Order? order = null)
    {
        Order = order;
    }
}
