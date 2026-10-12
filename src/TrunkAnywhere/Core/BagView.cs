namespace TrunkAnywhere.Core;

/// <summary>
/// What the game's bag screen shows. One screen serves both the player's own backpack alone and every two-sided view
/// (a storage, a station, the trunk), so which one is up is read from how the screen was last opened.
/// </summary>
public abstract record BagView
{
    private BagView()
    {
    }

    public sealed record Closed : BagView;

    /// <summary>The player's own backpack on its own, the one inventory the key may open over.</summary>
    public sealed record OwnBackpack : BagView;

    public sealed record Trunk : BagView;

    /// <summary>The backpack beside some other inventory: a storage, a station, a box.</summary>
    public sealed record OtherInventory(long OwnerId) : BagView;

    /// <summary>The screen is up but the mod never saw it open, so what it shows is not known.</summary>
    public sealed record Unknown : BagView;

    /// <summary>
    /// A screen that is down is closed whatever was last seen, so a missed close can never hold the key off for good.
    /// A screen that is up but was never seen opening is unknown, and the key treats that as another inventory.
    /// </summary>
    public static BagView Classify(bool isScreenUp, SeenScreen? lastOpened, long trunkOwnerId)
    {
        if (!isScreenUp) return new Closed();
        if (lastOpened == null) return new Unknown();
        if (!lastOpened.IsTwoSided) return new OwnBackpack();

        return lastOpened.OtherOwnerId == trunkOwnerId ? new Trunk() : new OtherInventory(lastOpened.OtherOwnerId);
    }
}

/// <summary>How the bag screen was last opened: alone, or beside the inventory of OtherOwnerId.</summary>
public sealed record SeenScreen(bool IsTwoSided, long OtherOwnerId);
