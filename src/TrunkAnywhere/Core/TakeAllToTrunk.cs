using System.Collections.Generic;
using System.Linq;

namespace TrunkAnywhere.Core;

/// <summary>The bag screen as the plugin reads it on the frame after the page's Take All to Trunk button is pressed.</summary>
public abstract record TakeAllRead
{
    private TakeAllRead()
    {
    }

    public sealed record Closed : TakeAllRead;

    /// <param name="ShownOwnerId">The container behind the left side's current tab.</param>
    /// <param name="OtherOwnerId">The container on the right side, the one being looted.</param>
    /// <param name="TabOwnerIds">The containers behind every tab on the left side, the trunk among them when it is in reach.</param>
    /// <param name="OtherHoldsUnpaid">Whether any item on the right side is a shop item not yet paid for.</param>
    public sealed record Open(bool IsDualBag, long ShownOwnerId, long OtherOwnerId, IReadOnlyList<long> TabOwnerIds, int OtherItemCount, bool OtherHoldsUnpaid) : TakeAllRead;
}

/// <summary>What one press of the page's Take All to Trunk button does.</summary>
public abstract record TakeAllDecision
{
    private TakeAllDecision()
    {
    }

    /// <summary>The game's own whole-container move, the one its Take All button sends, aimed at the trunk.</summary>
    public sealed record Move(long FromOwnerId, long ToOwnerId) : TakeAllDecision;

    /// <summary>Nothing moves, and the player is told why.</summary>
    public sealed record RefuseUnpaid : TakeAllDecision;

    /// <summary>Nothing moves; the reason goes to the log only, since the page does not show the button in these cases.</summary>
    public sealed record Ignore(TakeAllIgnoreReason Reason) : TakeAllDecision;
}

public enum TakeAllIgnoreReason
{
    ScreenClosed,
    NotDualBag,
    OtherIsTrunk,
    NoTrunkTab,
    TrunkTabShown,
    NothingToTake,
}

/// <summary>
/// The page's Take All to Trunk button moves everything on the right side of the bag screen into the trunk, when the
/// trunk is one of the left side's tabs and some other tab is shown. The game's whole-container move does not look at
/// the unpaid flag, so a right side holding any unpaid item moves nothing at all: unpaid items never enter the trunk.
/// </summary>
public static class TakeAllToTrunk
{
    public static TakeAllDecision Decide(TakeAllRead read, long trunkOwnerId)
    {
        if (read is not TakeAllRead.Open open) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.ScreenClosed);

        if (!open.IsDualBag) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.NotDualBag);

        if (open.OtherOwnerId == trunkOwnerId) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.OtherIsTrunk);

        if (!open.TabOwnerIds.Contains(trunkOwnerId)) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.NoTrunkTab);

        if (open.ShownOwnerId == trunkOwnerId) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.TrunkTabShown);

        if (open.OtherItemCount == 0) return new TakeAllDecision.Ignore(TakeAllIgnoreReason.NothingToTake);

        if (open.OtherHoldsUnpaid) return new TakeAllDecision.RefuseUnpaid();

        return new TakeAllDecision.Move(open.OtherOwnerId, trunkOwnerId);
    }
}
