namespace OrdersTest;

using System.Diagnostics.CodeAnalysis;
using Orders;
using FluentAssertions;

public class OrderTest
{
    [Fact]
    public void Constructor_SetsId()
    {
        var order = new Order();

        order.Id.Should().NotBe(Ulid.Empty);
    }

    [Fact]
    public void Constructor_SetsDifferentIds()
    {
        var order1 = new Order();
        var order2 = new Order();

        order1.Id.Should().NotBe(order2.Id);
    }

    [Fact]
    public void Constructor_SetsOrderStatusToNew()
    {
        var order = new Order();

        order.Status.Should().Be(OrderStatus.New);
    }

    [Fact]
    public void Constructor_SetsOrderPriorityToMedium()
    {
        var order = new Order();

        order.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void Constructor_AddsOrderToOrders()
    {
        var order = new Order();

        (Order.GetOrder(order.Id) == order).Should().BeTrue();

        Order.TryGetOrder(order.Id, out Order? retrievedOrder).Should().BeTrue();
        (retrievedOrder == order).Should().BeTrue();
    }

    [Fact]
    public void GetOrder_ThrowsExceptionForNonExistentOrder()
    {
        Action action = () => Order.GetOrder(Ulid.NewUlid());

        action.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void TryGetOrder_ReturnsFalseForNonExistentOrder()
    {
        Order.TryGetOrder(Ulid.NewUlid(), out Order? order).Should().BeFalse();
        order.Should().BeNull();
    }

    [Fact]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    public void Equal_DifferentOrdersAreNotEqual()
    {
        var order1 = new Order();
        var order2 = new Order();

        (order1.Id == order2.Id).Should().BeFalse();
        order1.Equals(order2).Should().BeFalse();
        (order1 == order2).Should().BeFalse();
    }

    [Fact]
    public void Add_AddsOrderLine()
    {
        var order = new Order();
        const string product = "Product";
        const int quantity = 1;

        OrderLine orderLine = order.Add(product, quantity);

        order[0].Should().Be(orderLine);

        orderLine.Product.Should().Be(product);
        orderLine.Quantity.Should().Be(quantity);
        order.Lines.Should().Contain(orderLine);
    }

    [Fact]
    public void Add_SetsOrderLineOrder()
    {
        var order = new Order();
        OrderLine orderLine = new OrderLine();

        order.Add(orderLine);

        (orderLine.Order == order).Should().BeTrue();
    }
}
