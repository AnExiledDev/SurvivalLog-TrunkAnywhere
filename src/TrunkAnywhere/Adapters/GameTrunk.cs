using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameCore.HotUpdate.Battle.Show;
using GameCore.HotUpdate.ReduxUI;
using Il2CppInterop.Runtime;
using TrunkAnywhere.Core;
using UnityEngine;
using Il2CppHeld = Il2CppSystem.Collections.Generic.Dictionary<long, GameCore.HotUpdate.ReduxUI.Data_Item>;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// The game's side of the trunk key and the Take All to Trunk button: what is open, the game's own trunk open path, and
/// its whole-container move. Reads go through singletons the game already made; actions are only ever sent from the ticker's frame, never from inside a reducer. Each call fails
/// soft, logging once per kind of call.
/// </summary>
internal sealed class GameTrunk
{
    private readonly Action<string> _log;
    private readonly HashSet<string> _reportedFailures = new();

    public GameTrunk(Action<string> log)
    {
        _log = log;
    }

    /// <summary>
    /// A loaded game is a world with a leading role whose disaster stage is set. VehicleConfig.HasVehicle reads the
    /// save's singleton and builds it if missing, so it is only asked once the world is known to be loaded.
    /// </summary>
    public PressRead Read()
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var role = world?._AgentManager?.GetLeadingRole();
        var ui = GameSingleton.Existing<ReduxUISystem>();
        var stage = ToStage(world?._GameTimeManager?.WorldStateType);

        if (world == null || role == null || ui == null || stage is not { } loadedStage) return new PressRead.NoGame();

        return new PressRead.InGame(new GameState(
            loadedStage,
            VehicleConfig.HasVehicle(),
            HotKeyManager.IsUGUIInputFocused(),
            InputHooks.IsGameInputOff,
            ui.HasModalUIOpenExcept(Il2CppType.Of<WebUI_BackpackUI>()),
            ReadBag(ui)));
    }

    /// <summary>A bag screen that cannot be read counts as unknown, which keeps a waiting press waiting rather than acting.</summary>
    public BagView ReadBag()
    {
        try
        {
            var ui = GameSingleton.Existing<ReduxUISystem>();

            return ui == null ? new BagView.Unknown() : ReadBag(ui);
        }
        catch (Exception ex)
        {
            ReportOnce("bag screen read", ex);

            return new BagView.Unknown();
        }
    }

    /// <summary>The game's UI names for what is open, for the log line of a refused press.</summary>
    public string OpenWindowNames()
    {
        try
        {
            var names = new Il2CppSystem.Collections.Generic.List<string>();
            var listed = new List<string>();

            GameSingleton.Existing<ReduxUISystem>()?.GetOpenUITypeNames(names);

            for (var i = 0; i < names.Count; i++) listed.Add(names[i]);

            return listed.Count == 0 ? "none" : string.Join(", ", listed);
        }
        catch (Exception ex)
        {
            ReportOnce("open window names", ex);

            return "unreadable";
        }
    }

    public bool IsKeyTakenByGame(KeyCode key)
    {
        try
        {
            return HotKeyManager.IsKeyTakenByOtherAction(key);
        }
        catch (Exception ex)
        {
            ReportOnce("key binding check", ex);

            return false;
        }
    }

    /// <summary>The open bag screen's two sides, read from the screen's live state and the UI store's item cache.</summary>
    public TakeAllRead ReadTakeAll()
    {
        var ui = GameSingleton.Existing<ReduxUISystem>();
        var screen = Il2CppType.Of<WebUI_BackpackUI>();

        if (ui == null || !ui.IsUIActive(screen) || ui.GetStateByUI(screen)?.TryCast<State_Web_BackpackUI>() is not { } bag) return new TakeAllRead.Closed();

        var other = HeldBy(ui, bag.OwnerId_B);

        return new TakeAllRead.Open(bag.IsDualBag, bag.OwnerId_A, bag.OwnerId_B, TabOwnerIds(bag), other?.Count ?? 0, HoldsUnpaid(other));
    }

    /// <summary>The game's own whole-container move, which its Take All button sends; the game's own lock checks still apply.</summary>
    public void MoveBag(long fromOwnerId, long toOwnerId) => Send("bag move", () => Ac_Item_MoveBag.SendAction(fromOwnerId, toOwnerId));

    public void CloseBackpack() => Send("backpack close", () => Ac_BackpackUI_Close.SendAction());

    /// <summary>
    /// The same event the cart's organize trunk action raises (AE_OpenVehicleTrunk). The leading role answers it by
    /// syncing the trunk into the UI and opening the bag screen beside it. Its handler builds the world if missing, so
    /// the event is only raised while the world is there.
    /// </summary>
    public void AskForTrunk()
    {
        if (GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole() == null)
        {
            _log("the trunk was not asked for: the game world is gone");

            return;
        }

        Send("trunk open", () => EventDispatcherTools.NotifyGlobal(EnumNotify_Vehicle.VehicleTrunkOpen));
    }

    /// <summary>Redraws every bag on screen from the unchanged item state, then says why the item did not move.</summary>
    public void ShowMoveRefused()
    {
        Send("bag refresh", () => Ac_Item_RefreshAllBagViews.SendAction());
        Send("pop text", () => Ac_PopText_AddPopText.SendAction(Texts.Blocked.Pick(GameLanguage.IsChinese())));
    }

    private static BagView ReadBag(ReduxUISystem ui) =>
        BagView.Classify(ui.IsUIActive(Il2CppType.Of<WebUI_BackpackUI>()), BackpackHooks.LastOpened, VehicleConfig.TrunkOwnerId);

    private static List<long> TabOwnerIds(State_Web_BackpackUI bag)
    {
        var tabs = bag.ATabOwnerIds;
        var ids = new List<long>();

        for (var i = 0; tabs != null && i < tabs.Count; i++) ids.Add(tabs[i]);

        return ids;
    }

    /// <summary>The items one container holds in the UI store's cache, the cache the bag screen draws from; null when it holds none.</summary>
    private static Il2CppHeld? HeldBy(ReduxUISystem ui, long ownerId)
    {
        var owners = ui.GetData<State_Data_Item>(Il2CppType.Of<State_Data_Item>())?.OwnerCache;

        return owners != null && owners.TryGetValue(ownerId, out var held) ? held : null;
    }

    /// <summary>The same unpaid flag the unpaid guard reads for a single move.</summary>
    private static bool HoldsUnpaid(Il2CppHeld? held)
    {
        if (held == null) return false;

        foreach (var item in held.Values)
        {
            if (item != null && item.IsShopItem) return true;
        }

        return false;
    }

    private static WorldStage? ToStage(WorldStateType? state) => state switch
    {
        WorldStateType.PreDisaster => WorldStage.BeforeDisaster,
        WorldStateType.PostDisaster => WorldStage.AfterDisaster,
        _ => null,
    };

    private void Send(string operation, Action send)
    {
        try
        {
            send();
        }
        catch (Exception ex)
        {
            ReportOnce(operation, ex);
        }
    }

    private void ReportOnce(string operation, Exception ex)
    {
        if (_reportedFailures.Add(operation)) _log($"{operation} failed (each kind reported once): {ex}");
    }
}
