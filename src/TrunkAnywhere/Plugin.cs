using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TrunkAnywhere.Adapters;
using UnityEngine;

namespace TrunkAnywhere;

[BepInPlugin(Guid, "Trunk Anywhere", Version)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.trunkanywhere";
    private const string Version = "0.1.1";

    public override void Load()
    {
        var trunkKey = Config.Bind("Controls", "TrunkKey", KeyCode.F6, "Key that opens the backpack and trunk view anywhere before the disaster, once you own a vehicle. It does nothing while a storage, shop or any other window is open, or while you type. None turns the key off; unpaid items are kept out of the trunk either way.").Value;
        var log = new TrunkLog(Log, Path.Combine(Paths.BepInExRootPath, "trunk-anywhere.log"));
        var harmony = new Harmony(Guid);

        log.Write($"===== Trunk Anywhere {Version} started, key {trunkKey}");
        UnpaidGuard.Apply(harmony, log.Write);
        BackpackHooks.Apply(harmony, log.Write);
        InputHooks.Apply(harmony, log.Write);
        TrunkTicker.Session = new TrunkSession(new GameTrunk(log.Write), trunkKey, log.Write);
        TrunkTicker.Log = log.Write;
        AddComponent<TrunkTicker>();
    }
}
