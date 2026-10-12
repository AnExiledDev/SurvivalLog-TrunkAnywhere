using System;
using GameCore.HotUpdate.Battle.Show;
using HarmonyLib;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Keeps a copy of the game's own switch that stops all of its hotkeys (HotKeyManager._isInputDisabled, the bool
/// HotKeyManager.Update tests first), so the trunk key stays off whenever the game's keys are. A postfix, so it still
/// runs when another mod's prefix skips the game's update. Static because a Harmony patch has nowhere else to look.
/// </summary>
internal static class InputHooks
{
    private static Action<string> _log = _ => { };
    private static bool _isGameInputOff;
    private static bool _hasReportedHookFailure;

    public static bool IsGameInputOff => _isGameInputOff;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            harmony.Patch(AccessTools.Method(typeof(HotKeyManager), "Update"), postfix: new HarmonyMethod(typeof(InputHooks), nameof(AfterUpdate)));
            log("the key stays off whenever the game turns its own hotkeys off");
        }
        catch (Exception ex)
        {
            log("could not hook HotKeyManager.Update, so the key only knows about focused text fields, not the game's own input switch: " + ex.Message);
        }
    }

    private static void AfterUpdate(HotKeyManager __instance)
    {
        try
        {
            if (__instance != null) _isGameInputOff = __instance._isInputDisabled;
        }
        catch (Exception ex)
        {
            if (_hasReportedHookFailure) return;

            _hasReportedHookFailure = true;
            _log("reading the game's input switch failed (only the first failure is logged): " + ex);
        }
    }
}
