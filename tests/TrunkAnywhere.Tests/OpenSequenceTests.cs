using TrunkAnywhere.Core;
using Xunit;

namespace TrunkAnywhere.Tests;

public class OpenSequenceTests
{
    [Fact]
    public void A_refused_press_starts_nothing()
    {
        var tick = OpenSequence.Start(new PressDecision.Ignore(IgnoreReason.NoVehicle));

        Assert.Equal(new OpenTick(new OpenStep.Idle(), OpenCommand.None), tick);
    }

    [Fact]
    public void With_nothing_open_the_trunk_is_asked_for_at_once()
    {
        var tick = OpenSequence.Start(new PressDecision.OpenTrunk());

        Assert.Equal(OpenCommand.AskForTrunk, tick.Command);
    }

    [Fact]
    public void Over_the_backpack_the_trunk_is_asked_for_only_once_the_backpack_is_gone()
    {
        var start = OpenSequence.Start(new PressDecision.CloseBackpackThenOpen());
        var stillUp = OpenSequence.Advance(start.Next, new BagView.OwnBackpack());
        var closing = OpenSequence.Advance(stillUp.Next, new BagView.Unknown());
        var gone = OpenSequence.Advance(closing.Next, new BagView.Closed());

        Assert.Equal(OpenCommand.CloseBackpack, start.Command);
        Assert.Equal(OpenCommand.None, stillUp.Command);
        Assert.Equal(OpenCommand.None, closing.Command);
        Assert.Equal(OpenCommand.AskForTrunk, gone.Command);
    }

    [Fact]
    public void A_backpack_that_never_closes_is_given_up_on_without_asking_for_the_trunk()
    {
        var commands = Run(OpenSequence.Start(new PressDecision.CloseBackpackThenOpen()), new BagView.OwnBackpack(), OpenSequence.CloseFrameLimit + 5);

        Assert.DoesNotContain(OpenCommand.AskForTrunk, commands);
        Assert.Equal(OpenCommand.ReportBackpackStuck, commands.Last(command => command != OpenCommand.None));
    }

    [Fact]
    public void A_trunk_view_that_shows_ends_the_press()
    {
        var asked = OpenSequence.Start(new PressDecision.OpenTrunk());
        var shown = OpenSequence.Advance(asked.Next, new BagView.Trunk());

        Assert.Equal(new OpenTick(new OpenStep.Idle(), OpenCommand.ReportOpened), shown);
    }

    [Fact]
    public void A_trunk_view_that_never_shows_is_asked_for_once_more_and_then_reported()
    {
        var commands = Run(OpenSequence.Start(new PressDecision.OpenTrunk()), new BagView.Closed(), OpenSequence.OpenFrameLimit * 3);
        var acted = commands.Where(command => command != OpenCommand.None).ToArray();

        Assert.Equal(new[] { OpenCommand.AskForTrunkAgain, OpenCommand.ReportTrunkMissing }, acted);
    }

    [Fact]
    public void The_trunk_is_not_asked_for_again_over_a_window_the_player_opened_meanwhile()
    {
        var commands = Run(OpenSequence.Start(new PressDecision.OpenTrunk()), new BagView.OtherInventory(4242), OpenSequence.OpenFrameLimit * 3);

        Assert.DoesNotContain(OpenCommand.AskForTrunkAgain, commands);
        Assert.Contains(OpenCommand.ReportTrunkMissing, commands);
    }

    /// <summary>Advances a started press against an unchanging bag screen until it ends or the frames run out.</summary>
    private static List<OpenCommand> Run(OpenTick start, BagView bag, int frames)
    {
        var commands = new List<OpenCommand>();
        var step = start.Next;

        for (var frame = 0; frame < frames && step is not OpenStep.Idle; frame++)
        {
            var tick = OpenSequence.Advance(step, bag);

            commands.Add(tick.Command);
            step = tick.Next;
        }

        Assert.IsType<OpenStep.Idle>(step);

        return commands;
    }
}
