using FluentAssertions;
using MessageParser;
using Orders;

namespace MessagesTest;

public class AswmParserTest
{
    [Fact]
    public void ParseMessage_CanParseAswmMessage()
    {
        var wizard = new Wizard();
        var parser = new AswmParser();
        var message = parser.ParseMessage($"""
            H|\^|Magic and More|Enchanting Emporium|2024-05-21|12:34:56
            O|Medium|{wizard.Id}
            L|Healing Potion|4
            L|Mana Potion|2
            O|High|{wizard.Id}
            L|Scroll of Fireball|1
            L|Scroll of Teleportation|1
            """);

        message.Should().NotBeNull();
        message.Sender.Should().Be("Magic and More");
        message.Receiver.Should().Be("Enchanting Emporium");
        message.Orders.Should().HaveCount(2);
        message.Orders[0].Priority.Should().Be(Priority.Medium);
        message.Orders[0].Customer.Should().Be(wizard);
        message.Orders[0].Count.Should().Be(2);
        message.Orders[0][0].Product.Should().Be("Healing Potion");
        message.Orders[0][0].Quantity.Should().Be(4);
        message.Orders[0][1].Product.Should().Be("Mana Potion");
        message.Orders[0][1].Quantity.Should().Be(2);
        message.Orders[1].Priority.Should().Be(Priority.High);
        message.Orders[1].Customer.Should().Be(wizard);
        message.Orders[1].Count.Should().Be(2);
        message.Orders[1][0].Product.Should().Be("Scroll of Fireball");
        message.Orders[1][0].Quantity.Should().Be(1);
        message.Orders[1][1].Product.Should().Be("Scroll of Teleportation");
        message.Orders[1][1].Quantity.Should().Be(1);
    }
}
