using System;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using TrunkAnywhere.Core;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Remembers how the bag screen was last opened (alone, or beside which inventory) and forgets it when the screen
/// closes. These run inside the game's UI reducers, so they only record. Static because a Harmony patch has nowhere
/// else to look.
/// </summary>
internal static class BackpackHooks
{
    private static readonly object Gate = new();
    private static Action<string> _log = _ => { };
    private static SeenScreen? _lastOpened;
    private static bool _isWatching;
    private static bool _hasReportedHookFailure;

    /// <summary>
    /// The screen last opened, or null when it was closed since or never seen. Always null when a hook could not be
    /// placed, so an open screen then reads as unknown and the key stays off while it is up.
    /// </summary>
    public static SeenScreen? LastOpened
    {
        get
        {
            lock (Gate) return _isWatching ? _lastOpened : null;
        }
    }

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;
        var isOpenHooked = Patch(harmony, "RA_OpenUI", nameof(AfterOpen));
        // RA_Close is compiled into Trigger_RA_Close, so a patch on RA_Close itself would never run.
        var isCloseHooked = Patch(harmony, "Trigger_RA_Close", nameof(AfterClose));
        var isClearHooked = Patch(harmony, "Trigger_RA_ClearUI", nameof(AfterClose));
        _isWatching = isOpenHooked && isCloseHooked && isClearHooked;

        log(_isWatching
            ? "the bag screen is watched, so the key opens over your own backpack but never over a storage or the trunk"
            : "the bag screen is not watched, so the key works only while the bag screen is closed");
    }

    private static bool Patch(Harmony harmony, string method, string postfix)
    {
        try
        {
            harmony.Patch(AccessTools.Method(typeof(Reducer_Web_BackpackUI), method), postfix: new HarmonyMethod(typeof(BackpackHooks), postfix));

            return true;
        }
        catch (Exception ex)
        {
            _log($"could not hook {nameof(Reducer_Web_BackpackUI)}.{method}: {ex.Message}");

            return false;
        }
    }

    private static void AfterOpen(State_Web_BackpackUI __result) =>
        Record(() =>
        {
            if (__result != null) _lastOpened = new SeenScreen(__result.IsDualBag, __result.OwnerId_B);
        });

    private static void AfterClose() => Record(() => _lastOpened = null);

    /// <summary>An exception must never escape into the game's reducer, so a failing hook logs once and records nothing.</summary>
    private static void Record(Action record)
    {
        try
        {
            lock (Gate) record();
        }
        catch (Exception ex)
        {
            if (_hasReportedHookFailure) return;

            _hasReportedHookFailure = true;
            _log("a bag screen hook failed (only the first failure is logged): " + ex);
        }
    }
}
