namespace TrunkAnywhere.Core;

/// <summary>One item about to change inventory. IsUnpaid is the game's own unpaid flag on a shop item not yet paid for.</summary>
public sealed record ItemMove(long FromOwnerId, long ToOwnerId, bool IsUnpaid);

/// <summary>One shop purchase about to be created in the buyer's inventory. Every shop item starts unpaid, so there is no flag to read.</summary>
public sealed record ShopBuy(long BuyerOwnerId);

public enum MoveVerdict
{
    Allow,
    Block,
}

/// <summary>
/// Unpaid items never enter the trunk, by either way in: a move into the trunk from somewhere else, or a shop purchase
/// whose buyer is the trunk (the shop's trunk tab). An unpaid item already in the trunk, from before this rule or from
/// a save, can still be rearranged there or taken out.
/// </summary>
public static class MoveGuard
{
    public static MoveVerdict Decide(ItemMove move, long trunkOwnerId)
    {
        var isIntoTrunk = move.ToOwnerId == trunkOwnerId && move.FromOwnerId != trunkOwnerId;

        return move.IsUnpaid && isIntoTrunk ? MoveVerdict.Block : MoveVerdict.Allow;
    }

    public static MoveVerdict Decide(ShopBuy buy, long trunkOwnerId) =>
        buy.BuyerOwnerId == trunkOwnerId ? MoveVerdict.Block : MoveVerdict.Allow;
}
