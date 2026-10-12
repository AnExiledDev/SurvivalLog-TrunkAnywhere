using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class BagViewTests
{
    private const long TrunkId = 900;
    private const long StorageId = 4242;

    [Fact]
    public void A_screen_that_is_down_is_closed_even_when_a_storage_was_the_last_thing_seen_open()
    {
        Assert.Equal(new BagView.Closed(), BagView.Classify(false, new SeenScreen(true, StorageId), TrunkId));
    }

    [Fact]
    public void A_screen_opened_alone_is_the_players_own_backpack()
    {
        Assert.Equal(new BagView.OwnBackpack(), BagView.Classify(true, new SeenScreen(false, 0), TrunkId));
    }

    [Fact]
    public void A_screen_opened_beside_the_trunk_is_the_trunk()
    {
        Assert.Equal(new BagView.Trunk(), BagView.Classify(true, new SeenScreen(true, TrunkId), TrunkId));
    }

    [Fact]
    public void A_screen_opened_beside_anything_else_is_another_inventory()
    {
        Assert.Equal(new BagView.OtherInventory(StorageId), BagView.Classify(true, new SeenScreen(true, StorageId), TrunkId));
    }

    [Fact]
    public void A_screen_that_is_up_but_was_never_seen_opening_is_unknown()
    {
        Assert.Equal(new BagView.Unknown(), BagView.Classify(true, null, TrunkId));
    }
}
