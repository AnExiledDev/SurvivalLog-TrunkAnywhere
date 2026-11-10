using System;
using ExiledToolkit.Game;
using ExiledToolkit.Web;
using TrunkAnywhere.Core;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's pages sit in. It
/// adds the Take All to Trunk button to the bag screen. The web view can reload at any time and lose the script, so
/// every couple of seconds the page is asked whether it is still there, and the script is sent again when the page
/// says it is missing. The button's label and whether it shows go to the page whenever they change.
/// </summary>
internal sealed class TrunkPage
{
    private const string NoScriptMessage = PageMessage.Prefix + "noscript";

    private readonly WebPage _page;
    private bool _isTakeAllShown = true;

    /// <summary>The language and show setting the page holds now; null when it holds none.</summary>
    private (bool IsChinese, bool IsTakeAllShown)? _sentState;

    public TrunkPage(Action<string> log)
    {
        _page = new WebPage(typeof(TrunkPage).Assembly, "TrunkAnywhere.page.js", "window.__trunkanywhere", NoScriptMessage, log);
        PageInbox.Listen(NoScriptMessage, _ => Install());
    }

    /// <summary>Whether the page shows the Take All to Trunk button; a page that is already running hears it at the next check.</summary>
    public void ShowTakeAll(bool isShown) => _isTakeAllShown = isShown;

    /// <summary>Every frame; does nothing between checks.</summary>
    public void KeepInstalled(double now)
    {
        if (!_page.KeepInstalled(now)) return;

        SendState();
    }

    /// <summary>The state goes first, so the button shows the right label the first time it is drawn.</summary>
    private void Install()
    {
        _sentState = null;
        SendState();
        _page.Install();
    }

    // A language change in the game's settings reaches a page that is already running at the next check.
    private void SendState()
    {
        (bool IsChinese, bool IsTakeAllShown) state = (GameLanguage.IsChinese(), _isTakeAllShown);

        if (_sentState == state) return;

        if (_page.Run(StateLines(state.IsChinese, state.IsTakeAllShown))) _sentState = state;
    }

    private static string StateLines(bool isChinese, bool isTakeAllShown) =>
        "window.__trunkanywhereLabel = '" + Texts.TakeAllLabel.Pick(isChinese) + "';\n" +
        "window.__trunkanywhereShowTakeAll = " + (isTakeAllShown ? "true" : "false") + ";\n";
}
