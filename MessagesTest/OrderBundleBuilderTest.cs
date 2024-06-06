using FluentAssertions;
using Messages;
using Orders;

namespace MessagesTest;

public class OrderBundleBuilderTest
{
    [Fact]
    public void Builder_CanBuildValidOrderBundle()
    {
        var builder = new OrderBundleBuilder();
        OrderBundle orderBundle = builder
            .SetSender("sender")
            .SetReceiver("receiver")
            .StartOrder()
            .StartOrderLine()
            .SetProduct("product 1.1")
            .SetQuantity("1")
            .StartOrderLine()
            .SetProduct("product 1.2")
            .SetQuantity("2")
            .StartOrder()
            .SetPriority("High")
            .StartOrderLine()
            .SetProduct("product 2.1")
            .SetQuantity("3")
            .Build();

        orderBundle.Sender.Should().Be("sender");
        orderBundle.Receiver.Should().Be("receiver");
        orderBundle.Orders.Should().HaveCount(2);
        orderBundle.Orders[0].Priority.Should().Be(Priority.Medium);
        orderBundle.Orders[0].Count.Should().Be(2);
        orderBundle.Orders[0][0].Product.Should().Be("product 1.1");
        orderBundle.Orders[0][0].Quantity.Should().Be(1);
        orderBundle.Orders[0][1].Product.Should().Be("product 1.2");
        orderBundle.Orders[0][1].Quantity.Should().Be(2);
        orderBundle.Orders[1].Priority.Should().Be(Priority.High);
        orderBundle.Orders[1].Count.Should().Be(1);
        orderBundle.Orders[1][0].Product.Should().Be("product 2.1");
        orderBundle.Orders[1][0].Quantity.Should().Be(3);
    }

    [Fact]
    public void Builder_ThrowsExceptionWhenSenderNotSet()
    {
        var builder = new OrderBundleBuilder();
        builder.SetReceiver("receiver");
        builder.Invoking(b => b.Build())
            .Should().Throw<InvalidOperationException>().WithMessage("Sender not set");
    }

    [Fact]
    public void Builder_ThrowsExceptionWhenReceiverNotSet()
    {
        var builder = new OrderBundleBuilder();
        builder.SetSender("sender");
        builder.Invoking(b => b.Build())
            .Should().Throw<InvalidOperationException>().WithMessage("Receiver not set");
    }

    [Fact]
    public void Builder_ThrowsExceptionWhenProductNotSet()
    {
        var builder = new OrderBundleBuilder();
        builder.SetSender("sender");
        builder.SetReceiver("receiver");
        builder.StartOrder();
        builder.StartOrderLine();
        builder.SetQuantity("1");

        builder.Invoking(b => b.Build()).Should().Throw<InvalidOperationException>().WithMessage("Product not set");
    }

    [Fact]
    public void Builder_CanBuildValidOrderBundleFromText()
    {
        var builder = new OrderBundleBuilder();
        builder.NewSegment("Order");
        builder.SetField("Sender", "sender");
        builder.SetField("Receiver", "receiver");
        builder.NewSegment("OrderLine");
        builder.SetField("Product", "product 1.1");
        builder.SetField("Quantity", "1");
        builder.NewSegment("OrderLine");
        builder.SetField("Product", "product 1.2");
        builder.SetField("Quantity", "2");
        builder.NewSegment("Order");
        builder.SetField("Priority", "High");
        builder.NewSegment("OrderLine");
        builder.SetField("Product", "product 2.1");
        builder.SetField("Quantity", "3");
        OrderBundle orderBundle = builder.Build();

        orderBundle.Sender.Should().Be("sender");
        orderBundle.Receiver.Should().Be("receiver");
        orderBundle.Orders.Should().HaveCount(2);
        orderBundle.Orders[0].Priority.Should().Be(Priority.Medium);
        orderBundle.Orders[0].Count.Should().Be(2);
        orderBundle.Orders[0][0].Product.Should().Be("product 1.1");
        orderBundle.Orders[0][0].Quantity.Should().Be(1);
        orderBundle.Orders[0][1].Product.Should().Be("product 1.2");
        orderBundle.Orders[0][1].Quantity.Should().Be(2);
        orderBundle.Orders[1].Priority.Should().Be(Priority.High);
        orderBundle.Orders[1].Count.Should().Be(1);
        orderBundle.Orders[1][0].Product.Should().Be("product 2.1");
        orderBundle.Orders[1][0].Quantity.Should().Be(3);
    }
}
