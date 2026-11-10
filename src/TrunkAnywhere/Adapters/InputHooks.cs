using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Show;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Keeps a copy of the game's switch that stops all of its hotkeys (HotKeyManager._isInputDisabled, the bool
/// HotKeyManager.Update tests first), so the trunk key stays off whenever the game's keys are. A postfix, so it still
/// runs when another mod's prefix skips the game's update. Static because a Harmony patch has nowhere else to look.
/// </summary>
internal static class InputHooks
{
    private static PatchKit? _hooks;
    private static bool _isGameInputOff;

    public static bool IsGameInputOff => _isGameInputOff;

    public static void Apply(PatchKit hooks)
    {
        _hooks = hooks;
        hooks.Patch(
            typeof(InputHooks), typeof(HotKeyManager), "Update",
            postfix: nameof(AfterUpdate),
            done: "the key stays off whenever the game turns its own hotkeys off",
            consequence: "so the key only knows about focused text fields, not the game's input switch");
    }

    private static void AfterUpdate(HotKeyManager __instance)
    {
        try
        {
            if (__instance != null) _isGameInputOff = __instance._isInputDisabled;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("reading the game's input switch", ex);
        }
    }
}
