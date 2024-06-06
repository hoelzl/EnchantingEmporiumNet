using Orders;

namespace Messages;

public class OrderBundleBuilder
{
    private string? _sender = null;
    private string? _receiver = null;
    private readonly List<Order> _orders = new();
    private Order? _currentOrder;
    private OrderLine? _currentOrderLine = null;

    public void NewSegment(string segmentName)
    {
        switch (segmentName)
        {
            case "Header":
                // Do nothing, header is always active
                return;
            case "Order":
                StartOrder();
                return;
            case "OrderLine":
                StartOrderLine();
                return;
        }
    }

    public OrderBundleBuilder SetField(string action, string argument)
    {
        switch (action)
        {
            case "Sender":
                return SetSender(argument!);
            case "Receiver":
                return SetReceiver(argument!);
            case "Priority":
                return SetPriority(argument!);
            case "Customer":
                return SetCustomer(argument!);
            case "Product":
                return SetProduct(argument!);
            case "Quantity":
                return SetQuantity(argument!);
            default:
                throw new InvalidOperationException($"Unknown action: {action}");
        }
    }

    public OrderBundleBuilder SetSender(string sender)
    {
        if (_sender is not null)
        {
            throw new InvalidOperationException("Sender already set");
        }

        _sender = sender;
        return this;
    }

    public OrderBundleBuilder SetReceiver(string receiver)
    {
        if (_receiver is not null)
        {
            throw new InvalidOperationException("Receiver already set");
        }

        _receiver = receiver;
        return this;
    }

    public OrderBundleBuilder StartOrder()
    {
        FinishCurrentOrderIfNecessary();
        _currentOrder = new Order();
        return this;
    }

    public OrderBundleBuilder SetPriority(string priority)
    {
        AssertOrderInProgress();
        _currentOrder!.Priority = Enum.Parse<Priority>(priority);
        return this;
    }

    public OrderBundleBuilder SetCustomer(string customerId)
    {
        AssertOrderInProgress();
        _currentOrder!.Customer = Wizard.GetWizard(Ulid.Parse(customerId));
        return this;
    }

    public OrderBundleBuilder StartOrderLine()
    {
        AssertOrderInProgress();
        FinishCurrentOrderLineIfNecessary();

        _currentOrderLine = new OrderLine(_currentOrder);
        return this;
    }

    public OrderBundleBuilder SetProduct(string product)
    {
        AssertOrderLineInProgress();
        _currentOrderLine!.Product = product;
        return this;
    }

    public OrderBundleBuilder SetQuantity(string quantity)
    {
        AssertOrderLineInProgress();
        _currentOrderLine!.Quantity = int.Parse(quantity);
        return this;
    }

    public OrderBundle Build()
    {
        FinishCurrentOrderIfNecessary();

        if (_sender is null)
        {
            throw new InvalidOperationException("Sender not set");
        }

        if (_receiver is null)
        {
            throw new InvalidOperationException("Receiver not set");
        }

        return new OrderBundle
        {
            Sender = _sender, Receiver = _receiver, Orders = _orders
        };
    }

    private void FinishCurrentOrderIfNecessary()
    {
        if (_currentOrder is null) return;

        FinishCurrentOrderLineIfNecessary();
        _orders.Add(_currentOrder!);
        _currentOrder = null;
    }

    private void FinishCurrentOrderLineIfNecessary()
    {
        if (_currentOrderLine is null) return;

        if (_currentOrderLine.Product is null)
        {
            throw new InvalidOperationException("Product not set");
        }

        if (_currentOrderLine.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity not set");
        }

        _currentOrder!.Add(_currentOrderLine!);
        _currentOrderLine = null;
    }

    private void AssertOrderInProgress()
    {
        if (_currentOrder is null)
        {
            throw new InvalidOperationException("No order in progress");
        }
    }

    private void AssertOrderLineInProgress()
    {
        if (_currentOrder is null)
        {
            throw new InvalidOperationException("No order in progress");
        }

        if (_currentOrderLine is null)
        {
            throw new InvalidOperationException("No order line in progress");
        }
    }
}
