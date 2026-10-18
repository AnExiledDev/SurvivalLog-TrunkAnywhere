using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using TrunkAnywhere.Core;
using Il2CppItems = Il2CppSystem.Collections.Generic.Dictionary<long, Il2CppSystem.Collections.Generic.Dictionary<long, GameCore.HotUpdate.ReduxUI.Data_Item>>;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Keeps unpaid items from being dragged into the trunk at the one place an item changes inventory in the game's item
/// state: Reducer_Data_Item.RA_ChangePos, which every drag on the bag screen goes through and which does not look at the
/// unpaid flag itself. A refused move skips the game's method and hands its state back untouched, so the item stays
/// where it was. Buying on the shop's trunk tab is not a move and is left to the game, which bills the trunk's items
/// together with the bag's. This runs inside a reducer, so it only records; the session refreshes the bag views and
/// says why on the next frame. Static because a Harmony patch has nowhere else to look.
/// </summary>
internal static class UnpaidGuard
{
    private static readonly object Gate = new();
    private static readonly List<long> Blocked = new();
    private static Action<string> _log = _ => { };
    private static bool _hasReportedHookFailure;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            harmony.Patch(AccessTools.Method(typeof(Reducer_Data_Item), "RA_ChangePos"), prefix: new HarmonyMethod(typeof(UnpaidGuard), nameof(BeforeChangePos)));
            log("unpaid items can't be dragged into the trunk");
        }
        catch (Exception ex)
        {
            log("could not hook Reducer_Data_Item.RA_ChangePos, so unpaid items CAN be dragged into the trunk: " + ex.Message);
        }
    }

    /// <summary>The ids of items kept out of the trunk since the last call.</summary>
    public static IReadOnlyList<long> TakeBlocked()
    {
        lock (Gate)
        {
            if (Blocked.Count == 0) return Array.Empty<long>();

            var taken = Blocked.ToArray();

            Blocked.Clear();

            return taken;
        }
    }

    /// <returns>False to skip the game's move; true lets it run.</returns>
    private static bool BeforeChangePos(Ac_Item_ChangePos ac, State_Data_Item state, ref State_Data_Item __result)
    {
        try
        {
            if (ac == null || state == null) return true;

            var move = new ItemMove(ac.FromOwnerId, ac.ToOwnerId, IsUnpaid(state.OwnerCache, ac.FromOwnerId, ac.ItemId));

            if (MoveGuard.Decide(move, VehicleConfig.TrunkOwnerId) == MoveVerdict.Allow) return true;

            __result = state;

            lock (Gate) Blocked.Add(ac.ItemId);

            return false;
        }
        catch (Exception ex)
        {
            ReportHookFailure(ex);

            // Failing closed here would stop every item move in the game, which is worse than one unpaid item slipping in.
            return true;
        }
    }

    /// <summary>Looked up where the game's own move finds the item. An item not found there is left to the game, which refuses the move itself.</summary>
    private static bool IsUnpaid(Il2CppItems? owners, long fromOwnerId, long itemId)
    {
        if (owners == null || !owners.TryGetValue(fromOwnerId, out var held) || held == null) return false;

        return held.TryGetValue(itemId, out var item) && item != null && item.IsShopItem;
    }

    private static void ReportHookFailure(Exception ex)
    {
        if (_hasReportedHookFailure) return;

        _hasReportedHookFailure = true;
        _log("the unpaid item check failed and let the move through (only the first failure is logged): " + ex);
    }
}
