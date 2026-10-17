using System;
using System.IO;
using GameCore.HotUpdate.ReduxUI;
using TrunkAnywhere.Core;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// adds the Take All to Trunk button to the bag screen. The web view can reload at any time and lose the script, so the
/// script is sent again every couple of seconds; it returns at once when it is already running.
/// </summary>
internal sealed class WebPage
{
    private const float CheckSeconds = 2f;
    private const string ScriptResource = "TrunkAnywhere.page.js";

    private readonly Action<string> _log;
    private string? _script;
    private float _nextCheckAt;
    private bool _hasReportedInstall;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log)
    {
        _log = log;
    }

    public void KeepInstalled(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        if (!Run(LabelLine() + Script()) || _hasReportedInstall) return;

        _hasReportedInstall = true;
        _log("page script sent to the web UI");
    }

    private bool Run(string javaScript)
    {
        var webView = GameSingleton.Existing<ReduxUISystem>()?.GetWebUILayer()?.canvasWebViewPrefab?.WebView;

        if (webView == null) return false;

        try
        {
            webView.ExecuteJavaScript(javaScript, null);

            return true;
        }
        catch (Exception ex)
        {
            if (!_hasReportedRunFailure) _log("could not run script in the web UI (reported once): " + ex.Message);

            _hasReportedRunFailure = true;

            return false;
        }
    }

    // Set on every send, ahead of the script's guard, so a language change in the game's settings reaches a page that
    // is already running.
    private static string LabelLine() => "window.__trunkanywhereLabel = '" + Texts.TakeAllLabel.Pick(GameLanguage.IsChinese()) + "';\n";

    private string Script()
    {
        if (_script != null) return _script;

        using var stream = typeof(WebPage).Assembly.GetManifestResourceStream(ScriptResource) ?? throw new InvalidOperationException(ScriptResource + " is not embedded");
        using var reader = new StreamReader(stream);

        return _script = reader.ReadToEnd();
    }
}
