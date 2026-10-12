namespace TrunkAnywhere.Core;

/// <summary>What one press of the trunk key does.</summary>
public abstract record PressDecision
{
    private PressDecision()
    {
    }

    /// <summary>Nothing happens in the game; the reason goes to the log only.</summary>
    public sealed record Ignore(IgnoreReason Reason) : PressDecision;

    public sealed record OpenTrunk : PressDecision;

    /// <summary>
    /// The player's own backpack is up. The game's open path skips the open while the backpack page is still showing,
    /// so it is closed first and the trunk is asked for once it is gone.
    /// </summary>
    public sealed record CloseBackpackThenOpen : PressDecision;
}

public enum IgnoreReason
{
    NoGame,
    AfterDisaster,
    Typing,
    GameInputOff,
    NoVehicle,
    OtherWindowOpen,
    OtherInventoryOpen,
    TrunkAlreadyOpen,
    BagScreenUnknown,
}
