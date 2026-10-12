using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class MoveGuardTests
{
    private const long TrunkId = 900;
    private const long BackpackId = 17;
    private const long StorageId = 4242;

    [Fact]
    public void An_unpaid_item_moved_into_the_trunk_is_refused()
    {
        Assert.Equal(MoveVerdict.Block, MoveGuard.Decide(new ItemMove(BackpackId, TrunkId, IsUnpaid: true), TrunkId));
    }

    [Fact]
    public void An_unpaid_item_moved_into_the_trunk_from_a_storage_is_refused_too()
    {
        Assert.Equal(MoveVerdict.Block, MoveGuard.Decide(new ItemMove(StorageId, TrunkId, IsUnpaid: true), TrunkId));
    }

    [Fact]
    public void A_paid_item_moved_into_the_trunk_is_allowed()
    {
        Assert.Equal(MoveVerdict.Allow, MoveGuard.Decide(new ItemMove(BackpackId, TrunkId, IsUnpaid: false), TrunkId));
    }

    [Fact]
    public void An_unpaid_item_already_in_the_trunk_can_be_rearranged_there()
    {
        Assert.Equal(MoveVerdict.Allow, MoveGuard.Decide(new ItemMove(TrunkId, TrunkId, IsUnpaid: true), TrunkId));
    }

    [Fact]
    public void An_unpaid_item_can_be_taken_out_of_the_trunk()
    {
        Assert.Equal(MoveVerdict.Allow, MoveGuard.Decide(new ItemMove(TrunkId, BackpackId, IsUnpaid: true), TrunkId));
    }

    [Fact]
    public void An_unpaid_item_moved_anywhere_but_the_trunk_is_left_to_the_game()
    {
        Assert.Equal(MoveVerdict.Allow, MoveGuard.Decide(new ItemMove(BackpackId, StorageId, IsUnpaid: true), TrunkId));
    }

    [Fact]
    public void A_shop_buy_from_the_trunk_tab_is_refused()
    {
        Assert.Equal(MoveVerdict.Block, MoveGuard.Decide(new ShopBuy(BuyerOwnerId: TrunkId), TrunkId));
    }

    [Fact]
    public void A_shop_buy_into_the_bag_is_allowed()
    {
        Assert.Equal(MoveVerdict.Allow, MoveGuard.Decide(new ShopBuy(BuyerOwnerId: BackpackId), TrunkId));
    }
}
