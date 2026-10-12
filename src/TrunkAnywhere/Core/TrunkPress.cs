using System;

namespace TrunkAnywhere.Core;

/// <summary>
/// The rules for the trunk key. It opens the backpack and trunk view only before the disaster, with a vehicle bought,
/// with nothing open but possibly the player's own backpack, and never while the player types.
/// </summary>
public static class TrunkPress
{
    public static PressDecision Decide(PressRead read) => read switch
    {
        PressRead.NoGame => Ignore(IgnoreReason.NoGame),
        PressRead.InGame inGame => DecideInGame(inGame.State),
        _ => throw new InvalidOperationException("unhandled press read " + read),
    };

    private static PressDecision DecideInGame(GameState state)
    {
        if (state.Stage == WorldStage.AfterDisaster) return Ignore(IgnoreReason.AfterDisaster);
        if (state.IsTyping) return Ignore(IgnoreReason.Typing);
        if (state.IsGameInputOff) return Ignore(IgnoreReason.GameInputOff);
        if (!state.HasVehicle) return Ignore(IgnoreReason.NoVehicle);
        if (state.IsOtherWindowOpen) return Ignore(IgnoreReason.OtherWindowOpen);

        return state.Bag switch
        {
            BagView.Closed => new PressDecision.OpenTrunk(),
            BagView.OwnBackpack => new PressDecision.CloseBackpackThenOpen(),
            BagView.Trunk => Ignore(IgnoreReason.TrunkAlreadyOpen),
            BagView.OtherInventory => Ignore(IgnoreReason.OtherInventoryOpen),
            BagView.Unknown => Ignore(IgnoreReason.BagScreenUnknown),
            _ => throw new InvalidOperationException("unhandled bag view " + state.Bag),
        };
    }

    private static PressDecision Ignore(IgnoreReason reason) => new PressDecision.Ignore(reason);
}
