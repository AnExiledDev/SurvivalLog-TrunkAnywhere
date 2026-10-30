namespace TrunkAnywhere.Core;

/// <summary>What the game looked like at the moment the key was pressed.</summary>
public abstract record PressRead
{
    private PressRead()
    {
    }

    /// <summary>Title screen, loading, or a world whose disaster stage is not set yet.</summary>
    public sealed record NoGame : PressRead;

    public sealed record InGame(GameState State) : PressRead;
}

public enum WorldStage
{
    BeforeDisaster,
    AfterDisaster,
}

/// <summary>
/// IsTyping is a text field with the keyboard: the game's own, or one in any page or mod window (the latter seen through
/// Exiled's Toolkit 0.12.0 or newer). IsGameInputOff is the game's own switch that stops all of its hotkeys.
/// IsOtherWindowOpen is any window the game counts as modal apart from the bag screen: a shop, trade, a menu.
/// </summary>
public sealed record GameState(WorldStage Stage, bool HasVehicle, bool IsTyping, bool IsGameInputOff, bool IsOtherWindowOpen, BagView Bag);
