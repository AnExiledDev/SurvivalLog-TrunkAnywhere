namespace TrunkAnywhere.Core;

/// <summary>One item about to change inventory. IsUnpaid is the game's own unpaid flag on a shop item not yet paid for.</summary>
public sealed record ItemMove(long FromOwnerId, long ToOwnerId, bool IsUnpaid);

public enum MoveVerdict
{
    Allow,
    Block,
}

/// <summary>
/// An unpaid item is never moved into the trunk from somewhere else. One already there (bought on the shop's trunk tab,
/// which the game bills together with the bag) can still be rearranged there or taken out.
/// </summary>
public static class MoveGuard
{
    public static MoveVerdict Decide(ItemMove move, long trunkOwnerId)
    {
        var isIntoTrunk = move.ToOwnerId == trunkOwnerId && move.FromOwnerId != trunkOwnerId;

        return move.IsUnpaid && isIntoTrunk ? MoveVerdict.Block : MoveVerdict.Allow;
    }
}
