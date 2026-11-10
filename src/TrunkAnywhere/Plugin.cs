using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit;
using ExiledToolkit.Game;
using ExiledToolkit.Kit;
using ExiledToolkit.Web;
using HarmonyLib;
using TrunkAnywhere.Adapters;
using TrunkAnywhere.Core;
using UnityEngine;

namespace TrunkAnywhere;

[BepInPlugin(Guid, "Trunk Anywhere", Version)]
[BepInDependency(ExiledToolkit.Plugin.Guid, ">=0.19.0")]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.trunkanywhere";
    private const string Version = "0.4.5";

    public override void Load()
    {
        var trunkKey = Config.Bind("Controls", "TrunkKey", KeyCode.F6, "Key that opens the backpack and trunk view anywhere before the disaster, once you own a vehicle. It does nothing while a storage, shop or any other window is open, or while you type. None turns the key off.\n在灾难来临前、拥有车辆后，在任何地方打开背包和后备箱界面的按键。打开储物、商店或其他窗口时，或正在输入文字时不起作用。设为 None 可关闭此按键。");
        var takeAll = Config.Bind("Looting", "TakeAllToTrunk", true, "Show the Take All to Trunk button under Take by Type when you loot with your trunk as one of the tabs. Off hides the button. Default: on.\n搜刮时如果后备箱是标签页之一，在「分类取出」下方显示「全部放进后备箱」按钮。关闭后不显示这个按钮。默认开启。");
        var log = new ModLog(Log, "trunk-anywhere.log");
        var hooks = new PatchKit(new Harmony(Guid), log.Write);
        var session = new TrunkSession(new GameTrunk(log.Write), PageInbox.Listen(PageMessage.Prefix), log.Write);

        log.Write($"===== Trunk Anywhere {Version} started");
        UnpaidGuard.Apply(hooks);
        BackpackHooks.Apply(hooks, log.Write);
        InputHooks.Apply(hooks);
        var page = new TrunkPage(log.Write);
        LiveSettings.Watch(trunkKey, key =>
        {
            session.UseKey(key);
            log.Write($"trunk key {key}");
        });
        LiveSettings.Watch(takeAll, isOn =>
        {
            session.UseTakeAll(isOn);
            page.ShowTakeAll(isOn);
            log.Write($"take all to trunk button {(isOn ? "on" : "off")}");
        });
        TrunkTicker.Session = session;
        TrunkTicker.Page = page;
        TrunkTicker.Log = log.Write;
        AddComponent<TrunkTicker>();
    }
}
