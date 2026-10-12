using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;
using TrunkAnywhere.Core;
using Il2CppItems = Il2CppSystem.Collections.Generic.Dictionary<long, Il2CppSystem.Collections.Generic.Dictionary<long, GameCore.HotUpdate.ReduxUI.Data_Item>>;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Keeps unpaid items out of the trunk at the two places one could get in, both in the game's item state:
/// Reducer_Data_Item.RA_ChangePos, which every drag on the bag screen goes through and which does not look at the
/// unpaid flag itself, and Reducer_Data_Item.RA_AddShopItem, which creates a purchase in the buyer's inventory and is
/// where the shop's trunk tab puts it in the trunk. A refusal skips the game's method and hands its state back
/// untouched: a refused move leaves the item where it was, and a refused buy creates nothing, so there is no bill line,
/// no money taken and no shop stock used (the game takes stock only on the success result RA_AddShopItem would have
/// sent). This runs inside a reducer, so it only records; the session says why on the next frame. Static because a
/// Harmony patch has nowhere else to look.
/// </summary>
internal static class UnpaidGuard
{
    private static readonly object Gate = new();
    private static readonly List<long> Blocked = new();
    private static readonly List<RefusedBuy> BlockedBuys = new();
    private static Action<string> _log = _ => { };
    private static bool _hasReportedHookFailure;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            harmony.Patch(AccessTools.Method(typeof(Reducer_Data_Item), "RA_ChangePos"), prefix: new HarmonyMethod(typeof(UnpaidGuard), nameof(BeforeChangePos)));
            log("unpaid items are kept out of the trunk");
        }
        catch (Exception ex)
        {
            log("could not hook Reducer_Data_Item.RA_ChangePos, so unpaid items are NOT kept out of the trunk: " + ex.Message);
        }

        try
        {
            harmony.Patch(AccessTools.Method(typeof(Reducer_Data_Item), "RA_AddShopItem"), prefix: new HarmonyMethod(typeof(UnpaidGuard), nameof(BeforeAddShopItem)));
            log("the shop's trunk tab cannot buy into the trunk");
        }
        catch (Exception ex)
        {
            log("could not hook Reducer_Data_Item.RA_AddShopItem, so the shop's trunk tab still buys unpaid items into the trunk: " + ex.Message);
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

    /// <summary>The shop buys refused since the last call.</summary>
    public static IReadOnlyList<RefusedBuy> TakeBlockedBuys()
    {
        lock (Gate)
        {
            if (BlockedBuys.Count == 0) return Array.Empty<RefusedBuy>();

            var taken = BlockedBuys.ToArray();

            BlockedBuys.Clear();

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

    /// <returns>False to skip the game's buy, which then creates no item, no bill line and no charge; true lets it run.</returns>
    private static bool BeforeAddShopItem(Ac_Item_AddShopItem ac, State_Data_Item state, ref State_Data_Item __result)
    {
        try
        {
            if (ac == null || state == null) return true;

            if (MoveGuard.Decide(new ShopBuy(ac.BuyerId), VehicleConfig.TrunkOwnerId) == MoveVerdict.Allow) return true;

            __result = state;

            lock (Gate) BlockedBuys.Add(new RefusedBuy(ac.ItemConfigId, ac.Count));

            return false;
        }
        catch (Exception ex)
        {
            ReportHookFailure(ex);

            // Failing closed here would stop every shop purchase in the game, which is worse than one unpaid item landing in the trunk.
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
        _log("the unpaid item check failed and let the move or buy through (only the first failure is logged): " + ex);
    }
}

/// <summary>A shop buy kept out of the trunk: what the game's own item config calls it, and how many.</summary>
internal readonly record struct RefusedBuy(int ItemConfigId, int Count);
