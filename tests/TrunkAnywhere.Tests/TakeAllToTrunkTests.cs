using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class TakeAllToTrunkTests
{
    private const long TrunkId = 900;
    private const long BackpackId = 17;
    private const long CrateId = 4242;
    private const long DoorBoxId = 5151;

    [Fact]
    public void Everything_in_the_looted_container_moves_into_the_trunk()
    {
        var decision = TakeAllToTrunk.Decide(Looting(), TrunkId);

        Assert.Equal(new TakeAllDecision.Move(FromOwnerId: CrateId, ToOwnerId: TrunkId), decision);
    }

    [Fact]
    public void It_works_from_any_tab_but_the_trunk()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { ShownOwnerId = DoorBoxId }, TrunkId);

        Assert.Equal(new TakeAllDecision.Move(CrateId, TrunkId), decision);
    }

    [Fact]
    public void A_looted_container_holding_an_unpaid_item_moves_nothing_and_says_why()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { OtherHoldsUnpaid = true }, TrunkId);

        Assert.IsType<TakeAllDecision.RefuseUnpaid>(decision);
    }

    [Fact]
    public void An_empty_looted_container_moves_nothing()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { OtherItemCount = 0 }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.NothingToTake), decision);
    }

    [Fact]
    public void An_empty_looted_container_is_not_called_unpaid()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { OtherItemCount = 0, OtherHoldsUnpaid = true }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.NothingToTake), decision);
    }

    [Fact]
    public void With_the_trunk_tab_shown_the_game_s_own_Take_All_is_the_button_and_this_one_does_nothing()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { ShownOwnerId = TrunkId }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.TrunkTabShown), decision);
    }

    [Fact]
    public void Without_a_trunk_tab_nothing_moves()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { TabOwnerIds = new[] { BackpackId, DoorBoxId } }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.NoTrunkTab), decision);
    }

    [Fact]
    public void Looting_the_trunk_itself_moves_nothing()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { OtherOwnerId = TrunkId }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.OtherIsTrunk), decision);
    }

    [Fact]
    public void The_backpack_alone_has_nothing_to_take_from()
    {
        var decision = TakeAllToTrunk.Decide(Looting() with { IsDualBag = false }, TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.NotDualBag), decision);
    }

    [Fact]
    public void A_closed_bag_screen_moves_nothing()
    {
        var decision = TakeAllToTrunk.Decide(new TakeAllRead.Closed(), TrunkId);

        Assert.Equal(new TakeAllDecision.Ignore(TakeAllIgnoreReason.ScreenClosed), decision);
    }

    /// <summary>The backpack tab shown on the left, with the trunk as a second tab, and a crate of three paid items on the right.</summary>
    private static TakeAllRead.Open Looting() =>
        new(IsDualBag: true, ShownOwnerId: BackpackId, OtherOwnerId: CrateId, TabOwnerIds: new[] { BackpackId, TrunkId }, OtherItemCount: 3, OtherHoldsUnpaid: false);
}
