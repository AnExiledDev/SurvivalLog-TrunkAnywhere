using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class PageScriptTests
{
    [Fact]
    public void The_loot_view_gets_the_button_right_under_Take_by_Type()
    {
        var page = new PageScript();

        page.OpenLoot();

        Assert.True(page.Truth("isShown(trunkButton())"));
        Assert.Equal(page.Number("indexInParent(controls().querySelector('.category-dropdown-wrapper'))") + 1, page.Number("indexInParent(trunkButton())"));
    }

    [Fact]
    public void The_button_looks_like_the_game_s_own_take_buttons()
    {
        var page = new PageScript();

        page.OpenLoot();

        Assert.Equal("transfer-btn btn-dir-left ta-take-trunk", page.Text("trunkButton().className"));
        Assert.Equal("btn-arrow", page.Text("trunkButton().children[0].className"));
        Assert.Equal("btn-label", page.Text("trunkButton().children[1].className"));
        Assert.Equal("Take All to Trunk", page.Text("trunkButton().children[1].textContent"));
    }

    [Fact]
    public void The_button_label_follows_the_language_the_plugin_sends()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Run("window.__trunkanywhereLabel = '" + Texts.TakeAllLabel.Zh + "';");
        page.SendScriptAgain();
        page.Poll();

        Assert.Equal(Texts.TakeAllLabel.Zh, page.Text("trunkButton().children[1].textContent"));
        Assert.Equal(1, page.Number("controls().querySelectorAll('.ta-take-trunk').length"));
    }

    [Fact]
    public void Pressing_the_button_asks_the_plugin_to_take_all_to_the_trunk()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Press("trunkButton()");

        Assert.Equal(new PageMessage.TakeAllToTrunk(), Assert.Single(page.Posted));
    }

    [Fact]
    public void With_nothing_to_take_the_button_is_greyed_out_and_a_press_sends_nothing()
    {
        var page = new PageScript();

        page.OpenLoot("s.toSelfEmpty = true;");
        page.Press("trunkButton()");

        Assert.True(page.Truth("isShown(trunkButton()) && trunkButton().disabled"));
        Assert.Empty(page.Posted);
    }

    [Fact]
    public void The_button_comes_back_to_life_once_there_is_something_to_take()
    {
        var page = new PageScript();

        page.OpenLoot("s.toSelfEmpty = true;");
        page.Change("s.toSelfEmpty = false;");

        Assert.False(page.Truth("trunkButton().disabled"));
    }

    [Fact]
    public void With_the_trunk_tab_shown_the_game_s_own_Take_All_already_fills_the_trunk_so_the_button_hides()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Change("s.curATab = 1;");

        Assert.False(page.Truth("isShown(trunkButton())"));
    }

    [Fact]
    public void Switching_back_off_the_trunk_tab_shows_the_button_again()
    {
        var page = new PageScript();

        page.OpenLoot("s.curATab = 1;");
        page.Change("s.curATab = 0;");

        Assert.True(page.Truth("isShown(trunkButton())"));
    }

    [Fact]
    public void Without_a_trunk_tab_there_is_no_button()
    {
        var page = new PageScript();

        page.OpenLoot("s.aTabs = [s.aTabs[0]];");

        Assert.Equal(0, page.Number("trunkButtons().length"));
    }

    [Fact]
    public void A_trunk_tab_that_takes_nothing_in_gets_no_button()
    {
        var page = new PageScript();

        page.OpenLoot("s.aTabs[1].readonly = true;");

        Assert.False(page.Truth("isShown(trunkButton())"));
    }

    [Fact]
    public void Looting_the_trunk_itself_gets_no_button()
    {
        var page = new PageScript();

        page.OpenLoot("s.isTrunk = true;");

        Assert.False(page.Truth("isShown(trunkButton())"));
    }

    [Fact]
    public void The_backpack_on_its_own_gets_no_button()
    {
        var page = new PageScript();

        page.OpenLoot("s.isDualBag = false;");

        Assert.False(page.Truth("isShown(trunkButton())"));
    }

    [Fact]
    public void Polling_again_never_adds_a_second_button()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Poll();
        page.Poll();

        Assert.Equal(1, page.Number("trunkButtons().length"));
    }

    [Fact]
    public void A_redrawn_column_of_buttons_gets_the_button_back_once()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Run("redrawControls()");
        page.Poll();
        page.Poll();

        Assert.Equal(1, page.Number("trunkButtons().length"));
        Assert.True(page.Truth("trunkButton().parentElement === controls()"));
    }

    [Fact]
    public void A_bag_screen_whose_state_cannot_be_read_hides_the_button_and_says_so_once()
    {
        var page = new PageScript();

        page.OpenLoot();
        page.Run("bagDoc().getElementById('app')._vnode = null");
        page.Poll();
        page.Poll();

        Assert.False(page.Truth("isShown(trunkButton())"));
        Assert.Single(page.Posted, message => message is PageMessage.Diag);
    }

    [Fact]
    public void Sending_the_script_again_does_not_start_a_second_copy()
    {
        var page = new PageScript();

        page.Run("var firstPolls = intervals.length");
        page.SendScriptAgain();

        Assert.True(page.Truth("intervals.length === firstPolls"));
    }
}
