using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class TrunkPressTests
{
    /// <summary>A loaded game before the disaster, a vehicle bought, nothing open and no one typing.</summary>
    private static readonly GameState Ready = new(WorldStage.BeforeDisaster, HasVehicle: true, IsTyping: false, IsGameInputOff: false, IsOtherWindowOpen: false, new BagView.Closed());

    [Fact]
    public void With_a_vehicle_and_nothing_open_before_the_disaster_the_key_opens_the_trunk()
    {
        Assert.Equal(new PressDecision.OpenTrunk(), Press(Ready));
    }

    [Fact]
    public void Over_the_players_own_backpack_the_key_closes_it_and_then_opens_the_trunk()
    {
        Assert.Equal(new PressDecision.CloseBackpackThenOpen(), Press(Ready with { Bag = new BagView.OwnBackpack() }));
    }

    [Fact]
    public void Without_a_loaded_game_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.NoGame), TrunkPress.Decide(new PressRead.NoGame()));
    }

    [Fact]
    public void After_the_disaster_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.AfterDisaster), Press(Ready with { Stage = WorldStage.AfterDisaster }));
    }

    [Fact]
    public void Without_a_bought_vehicle_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.NoVehicle), Press(Ready with { HasVehicle = false }));
    }

    [Fact]
    public void Without_a_bought_vehicle_the_key_does_nothing_over_the_backpack_either()
    {
        Assert.Equal(Ignored(IgnoreReason.NoVehicle), Press(Ready with { HasVehicle = false, Bag = new BagView.OwnBackpack() }));
    }

    [Fact]
    public void With_a_storage_open_beside_the_backpack_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.OtherInventoryOpen), Press(Ready with { Bag = new BagView.OtherInventory(4242) }));
    }

    [Fact]
    public void With_the_trunk_already_open_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.TrunkAlreadyOpen), Press(Ready with { Bag = new BagView.Trunk() }));
    }

    [Fact]
    public void With_a_shop_trade_or_other_window_open_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.OtherWindowOpen), Press(Ready with { IsOtherWindowOpen = true }));
    }

    [Fact]
    public void With_another_window_open_over_the_players_backpack_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.OtherWindowOpen), Press(Ready with { IsOtherWindowOpen = true, Bag = new BagView.OwnBackpack() }));
    }

    [Fact]
    public void With_a_bag_screen_the_mod_did_not_see_open_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.BagScreenUnknown), Press(Ready with { Bag = new BagView.Unknown() }));
    }

    [Fact]
    public void While_a_text_field_has_focus_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.Typing), Press(Ready with { IsTyping = true }));
    }

    [Fact]
    public void While_the_game_has_turned_its_own_hotkeys_off_the_key_does_nothing()
    {
        Assert.Equal(Ignored(IgnoreReason.GameInputOff), Press(Ready with { IsGameInputOff = true }));
    }

    private static PressDecision Press(GameState state) => TrunkPress.Decide(new PressRead.InGame(state));

    private static PressDecision Ignored(IgnoreReason reason) => new PressDecision.Ignore(reason);
}
