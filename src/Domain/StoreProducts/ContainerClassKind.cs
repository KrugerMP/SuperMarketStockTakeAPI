namespace SuperMarketStockTakeAPI.Domain.StoreProducts;

/// <summary>
/// Behavioral category for <see cref="Data_Container.containerClass"/>.
/// The game stores arbitrary ints for shelf variants; only <see cref="Storage"/> and <see cref="Checkout"/> are branched on in <c>Data_Container</c> (69 and 99). All other values use the normal shelf path.
/// </summary>
public enum ContainerClassKind
{
    /// <summary>Normal retail shelf (<c>ItemSpawner</c>); any <c>containerClass</c> except 69 and 99.</summary>
    RetailShelf,

    /// <summary>Storage unit (<c>BoxSpawner</c>); <c>containerClass</c> == 69.</summary>
    Storage,

    /// <summary>Checkout-related; <c>containerClass</c> == 99.</summary>
    Checkout,
}

public static class ContainerClassKindExtensions
{
    public static ContainerClassKind FromGameClass(int containerClass)
    {
        return containerClass switch
        {
            69 => ContainerClassKind.Storage,
            99 => ContainerClassKind.Checkout,
            _ => ContainerClassKind.RetailShelf,
        };
    }
}