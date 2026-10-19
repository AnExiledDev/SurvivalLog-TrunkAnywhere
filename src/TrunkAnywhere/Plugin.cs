using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TrunkAnywhere.Adapters;
using UnityEngine;

namespace TrunkAnywhere;

[BepInPlugin(Guid, "Trunk Anywhere", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.trunkanywhere";
    private const string Version = "0.4.0";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var trunkKey = Config.Bind("Controls", "TrunkKey", KeyCode.F6, "Key that opens the backpack and trunk view anywhere before the disaster, once you own a vehicle. It does nothing while a storage, shop or any other window is open, or while you type. None turns the key off.\n在灾难来临前、拥有车辆后，在任何地方打开背包和后备箱界面的按键。打开储物、商店或其他窗口时，或正在输入文字时不起作用。设为 None 可关闭此按键。");
        var log = new TrunkLog(Log, Path.Combine(Paths.BepInExRootPath, "trunk-anywhere.log"));
        var harmony = new Harmony(Guid);
        var session = new TrunkSession(new GameTrunk(log.Write), log.Write);

        log.Write($"===== Trunk Anywhere {Version} started");
        UnpaidGuard.Apply(harmony, log.Write);
        BackpackHooks.Apply(harmony, log.Write);
        InputHooks.Apply(harmony, log.Write);
        PageMessages.Apply(harmony, log.Write);
        OptionalLive.Watch(trunkKey, key =>
        {
            session.UseKey(key);
            log.Write($"trunk key {key}");
        });
        TrunkTicker.Session = session;
        TrunkTicker.Page = new WebPage(log.Write);
        TrunkTicker.Log = log.Write;
        AddComponent<TrunkTicker>();
    }
}
