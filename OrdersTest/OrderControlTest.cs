namespace OrdersTest;

using Orders;
using FluentAssertions;

public class OrderControlActionsTest
{
    private readonly Wizard _customer = new(Ulid.NewUlid());
    private readonly Order _order = new();

    [Fact]
    public void NewOrderAction_Execute_CreatesNewOrder()
    {
        var action = new NewOrderAction(_customer);

        Order order = action.Execute();

        order.Customer.Should().Be(_customer);
        order.Count.Should().Be(0);
        order.Priority.Should().Be(Priority.Medium);
        order.Status.Should().Be(OrderStatus.New);

        Order.TryGetOrder(order.Id, out Order? retrievedOrder).Should().BeTrue();
        (retrievedOrder == order).Should().BeTrue();
    }

    [Fact]
    public void NewOrderAction_Execute_CreatesNewOrderWithCorrectPriority()
    {
        const Priority priority = Priority.High;
        var action = new NewOrderAction(_customer, priority);

        Order order = action.Execute();

        order.Priority.Should().Be(priority);
    }

    [Fact]
    public void CancelOrderAction_Execute_CancelsOrder()
    {
        var action = new CancelOrderAction(_order, "Reason");

        Order order = action.Execute();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void AddOrderLineAction_Execute_AddsOrderLine()
    {
        const string product = "Product";
        const int quantity = 1;
        var action = new AddOrderLineAction(_order, product, quantity);

        Order order = action.Execute();

        order.Count.Should().Be(1);
        OrderLine orderLine = order[0];
        orderLine.Product.Should().Be(product);
        orderLine.Quantity.Should().Be(quantity);
    }
}
