using System;

namespace TrunkAnywhere.Core;

/// <summary>Where an accepted press is on its way to the trunk view.</summary>
public abstract record OpenStep
{
    private OpenStep()
    {
    }

    public sealed record Idle : OpenStep;

    public sealed record WaitingForBackpackToClose(int FramesLeft) : OpenStep;

    public sealed record WaitingForTrunk(int FramesLeft, bool HasAskedAgain) : OpenStep;
}

public enum OpenCommand
{
    None,
    CloseBackpack,
    AskForTrunk,
    AskForTrunkAgain,
    ReportOpened,
    ReportBackpackStuck,
    ReportTrunkMissing,
}

public sealed record OpenTick(OpenStep Next, OpenCommand Command);

/// <summary>
/// Drives an accepted press one frame at a time. The trunk is asked for only once the bag screen is down, because the
/// game skips the open while its backpack page still shows. If the trunk view does not show, it is asked for once more
/// (the first ask may have landed while the page was still going away), and then the press is given up and reported.
/// Every wait is bounded, so a press can never hang.
/// </summary>
public static class OpenSequence
{
    public const int CloseFrameLimit = 60;
    public const int OpenFrameLimit = 30;

    public static OpenTick Start(PressDecision decision) => decision switch
    {
        PressDecision.OpenTrunk => new OpenTick(new OpenStep.WaitingForTrunk(OpenFrameLimit, false), OpenCommand.AskForTrunk),
        PressDecision.CloseBackpackThenOpen => new OpenTick(new OpenStep.WaitingForBackpackToClose(CloseFrameLimit), OpenCommand.CloseBackpack),
        PressDecision.Ignore => Done(OpenCommand.None),
        _ => throw new InvalidOperationException("unhandled press decision " + decision),
    };

    public static OpenTick Advance(OpenStep step, BagView bag) => step switch
    {
        OpenStep.Idle => Done(OpenCommand.None),
        OpenStep.WaitingForBackpackToClose waiting => AwaitClose(waiting, bag),
        OpenStep.WaitingForTrunk waiting => AwaitTrunk(waiting, bag),
        _ => throw new InvalidOperationException("unhandled open step " + step),
    };

    private static OpenTick AwaitClose(OpenStep.WaitingForBackpackToClose waiting, BagView bag)
    {
        if (bag is BagView.Closed) return new OpenTick(new OpenStep.WaitingForTrunk(OpenFrameLimit, false), OpenCommand.AskForTrunk);
        if (waiting.FramesLeft <= 1) return Done(OpenCommand.ReportBackpackStuck);

        return new OpenTick(waiting with { FramesLeft = waiting.FramesLeft - 1 }, OpenCommand.None);
    }

    private static OpenTick AwaitTrunk(OpenStep.WaitingForTrunk waiting, BagView bag)
    {
        if (bag is BagView.Trunk) return Done(OpenCommand.ReportOpened);
        if (waiting.FramesLeft > 1) return new OpenTick(waiting with { FramesLeft = waiting.FramesLeft - 1 }, OpenCommand.None);
        if (!waiting.HasAskedAgain && bag is BagView.Closed) return new OpenTick(new OpenStep.WaitingForTrunk(OpenFrameLimit, true), OpenCommand.AskForTrunkAgain);

        return Done(OpenCommand.ReportTrunkMissing);
    }

    private static OpenTick Done(OpenCommand command) => new(new OpenStep.Idle(), command);
}
