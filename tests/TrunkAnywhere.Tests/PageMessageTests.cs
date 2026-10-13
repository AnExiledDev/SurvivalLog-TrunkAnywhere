using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class PageMessageTests
{
    [Fact]
    public void The_button_press_is_read_as_take_all_to_trunk()
    {
        Assert.IsType<PageMessage.TakeAllToTrunk>(PageMessage.Parse("slmod|trunkanywhere|takeAllToTrunk"));
    }

    [Fact]
    public void A_page_report_keeps_its_whole_text_bars_included()
    {
        Assert.Equal(new PageMessage.Diag("poll: a|b"), PageMessage.Parse("slmod|trunkanywhere|diag|poll: a|b"));
    }

    [Fact]
    public void Another_mod_s_message_is_not_ours()
    {
        Assert.False(PageMessage.IsOurs("slmod|storagehub|putAwayBag"));
        Assert.False(PageMessage.IsOurs(null));
    }

    [Fact]
    public void A_verb_this_version_does_not_know_is_not_acted_on()
    {
        Assert.IsType<PageMessage.Unknown>(PageMessage.Parse("slmod|trunkanywhere|takeAllToTrunk|now"));
        Assert.IsType<PageMessage.Unknown>(PageMessage.Parse("slmod|trunkanywhere|dropAll"));
    }
}
