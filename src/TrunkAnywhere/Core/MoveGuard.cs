namespace TrunkAnywhere.Core;

/// <summary>One item about to change inventory. IsUnpaid is the game's own unpaid flag on a shop item not yet paid for.</summary>
public sealed record ItemMove(long FromOwnerId, long ToOwnerId, bool IsUnpaid);

public enum MoveVerdict
{
    Allow,
    Block,
}

/// <summary>
/// Unpaid items never enter the trunk. Only a move into the trunk from somewhere else is refused: an unpaid item
/// already there (the shop's trunk tab buys straight into it) can still be rearranged or taken out.
/// </summary>
public static class MoveGuard
{
    public static MoveVerdict Decide(ItemMove move, long trunkOwnerId)
    {
        var isIntoTrunk = move.ToOwnerId == trunkOwnerId && move.FromOwnerId != trunkOwnerId;

        return move.IsUnpaid && isIntoTrunk ? MoveVerdict.Block : MoveVerdict.Allow;
    }
}
