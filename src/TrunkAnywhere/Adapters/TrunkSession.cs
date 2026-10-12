using System;
using TrunkAnywhere.Core;
using UnityEngine;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// The mod's game side, run once a frame: answers the trunk key, walks an accepted press through to the trunk view,
/// and reports moves and shop buys the unpaid guard refused. A refused press writes one log line and touches nothing in the game.
/// </summary>
internal sealed class TrunkSession
{
    private readonly GameTrunk _game;
    private readonly KeyCode _trunkKey;
    private readonly Action<string> _log;
    private OpenStep _step = new OpenStep.Idle();
    private bool _hasCheckedKeyBinding;

    public TrunkSession(GameTrunk game, KeyCode trunkKey, Action<string> log)
    {
        _game = game;
        _trunkKey = trunkKey;
        _log = log;
    }

    public void Update()
    {
        ReportRefusedMoves();
        ReportRefusedBuys();

        if (_trunkKey != KeyCode.None && Input.GetKeyDown(_trunkKey))
        {
            Press();

            return;
        }

        if (_step is not OpenStep.Idle) Advance();
    }

    private void ReportRefusedMoves()
    {
        var blocked = UnpaidGuard.TakeBlocked();

        if (blocked.Count == 0) return;

        _log($"kept {blocked.Count} unpaid item(s) out of the trunk: {string.Join(", ", blocked)}");
        _game.ShowMoveRefused();
    }

    private void ReportRefusedBuys()
    {
        var refused = UnpaidGuard.TakeBlockedBuys();

        if (refused.Count == 0) return;

        _log($"kept {refused.Count} shop buy(s) out of the trunk, nothing bought or charged: {string.Join(", ", refused)}");
        _game.ShowBuyRefused();
    }

    /// <summary>The frame of an accepted press only starts it; waiting starts on the next frame, after the game has handled what was sent.</summary>
    private void Press()
    {
        if (_step is not OpenStep.Idle)
        {
            _log("press: ignored, the trunk is still being opened");

            return;
        }

        try
        {
            var read = _game.Read();
            var decision = TrunkPress.Decide(read);

            _log($"press: {decision}{OpenWindowsNote(decision)}");
            WarnOnceIfKeyTaken(read);
            Run(OpenSequence.Start(decision));
        }
        catch (Exception ex)
        {
            _step = new OpenStep.Idle();

            _log("press failed: " + ex);
        }
    }

    private void Advance()
    {
        try
        {
            Run(OpenSequence.Advance(_step, _game.ReadBag()));
        }
        catch (Exception ex)
        {
            _step = new OpenStep.Idle();

            _log("opening the trunk failed: " + ex);
        }
    }

    private void Run(OpenTick tick)
    {
        _step = tick.Next;

        switch (tick.Command)
        {
            case OpenCommand.None:
                break;
            case OpenCommand.CloseBackpack:
                _game.CloseBackpack();
                break;
            case OpenCommand.AskForTrunk:
                _game.AskForTrunk();
                break;
            case OpenCommand.AskForTrunkAgain:
                _log($"the trunk view did not show within {OpenSequence.OpenFrameLimit} frames, asking once more");
                _game.AskForTrunk();
                break;
            case OpenCommand.ReportOpened:
                _log("trunk view open");
                break;
            case OpenCommand.ReportBackpackStuck:
                _log($"your backpack did not close within {OpenSequence.CloseFrameLimit} frames, so the trunk was not opened");
                break;
            case OpenCommand.ReportTrunkMissing:
                _log($"the trunk view did not show after two asks, or opened without the mod seeing it; bag screen now {_game.ReadBag()}");
                break;
            default:
                throw new InvalidOperationException("unhandled open command " + tick.Command);
        }
    }

    /// <summary>A refused press in a loaded game names what was open, so a wrong refusal can be traced from the one log line.</summary>
    private string OpenWindowsNote(PressDecision decision) =>
        decision is PressDecision.Ignore { Reason: not IgnoreReason.NoGame } ? $", open: {_game.OpenWindowNames()}" : "";

    private void WarnOnceIfKeyTaken(PressRead read)
    {
        if (_hasCheckedKeyBinding || read is not PressRead.InGame) return;

        _hasCheckedKeyBinding = true;

        if (_game.IsKeyTakenByGame(_trunkKey)) _log($"{_trunkKey} is also bound to one of the game's own actions; pick another TrunkKey or rebind that action in the game's key settings");
    }
}
