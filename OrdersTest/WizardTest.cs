using FluentAssertions;
using Orders;

namespace OrdersTest;

public class WizardTest
{
    [Fact]
    public void Constructor_SetsId()
    {
        var wizard = new Wizard();

        wizard.Id.Should().NotBe(Ulid.Empty);
    }

    [Fact]
    public void Constructor_SetsDifferentIds()
    {
        var wizard1 = new Wizard();
        var wizard2 = new Wizard();

        wizard1.Id.Should().NotBe(wizard2.Id);
    }

    [Fact]
    public void Constructor_AddsWizardToWizards()
    {
        var wizard = new Wizard();

        Wizard.TryGetWizard(wizard.Id, out Wizard? retrievedWizard).Should().BeTrue();
        retrievedWizard.Should().Be(wizard);
    }

    [Fact]
    public void GetWizard_ReturnsWizard()
    {
        var wizard = new Wizard();

        Wizard retrievedWizard = Wizard.GetWizard(wizard.Id);

        retrievedWizard.Should().Be(wizard);
    }

    [Fact]
    public void GetWizard_ThrowsExceptionForNonExistentWizard()
    {
        Action action = () => Wizard.GetWizard(Ulid.NewUlid());

        action.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void TryGetWizard_ReturnsExistingWizard()
    {
        var wizard = new Wizard();

        Wizard.TryGetWizard(wizard.Id, out Wizard? retrievedWizard).Should().BeTrue();

        retrievedWizard.Should().Be(wizard);
    }

    [Fact]
    public void TryGetWizard_ReturnsFalseForNonExistentWizard()
    {
        Wizard.TryGetWizard(Ulid.NewUlid(), out Wizard? retrievedWizard).Should().BeFalse();
        retrievedWizard.Should().BeNull();
    }
}
