using Jint;
using TrunkAnywhere.Core;

namespace TrunkAnywhere.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint with Exiled's Toolkit's page API (api.js, from the Toolkit's
/// source) on top of FakePage.js (Root.html with the bag screen in an iframe, the vuplex bridge, a poll). What the
/// script posts is parsed with PageMessage, as the plugin parses it.
/// </summary>
internal sealed class PageScript
{
    private readonly Engine _engine = new();

    /// <param name="isToolkitFirst">
    /// The game runs the Toolkit's script first, or with false Trunk Anywhere's first; then the Toolkit's runs only at
    /// <see cref="LoadToolkit"/>.
    /// </param>
    public PageScript(bool isToolkitFirst = true)
    {
        _engine.Execute(Resource("TrunkAnywhere.Tests.FakePage.js"));

        if (isToolkitFirst) LoadToolkit();

        _engine.Execute(Resource("TrunkAnywhere.page.js"));
        Hear();
    }

    /// <summary>The Toolkit's page API arriving after Trunk Anywhere's script.</summary>
    public void LoadToolkit() => Run(Resource("ExiledToolkit.api.js"));

    /// <summary>Everything the script posted to the plugin, parsed as the plugin parses it.</summary>
    public List<PageMessage> Posted { get; } = new();

    public void Run(string statements)
    {
        _engine.Execute(statements);
        Hear();
    }

    public string Text(string expression) => _engine.Evaluate($"String({expression})").AsString();

    public double Number(string expression) => _engine.Evaluate($"Number({expression})").AsNumber();

    public bool Truth(string expression) => _engine.Evaluate($"Boolean({expression})").AsBoolean();

    /// <summary>One run of the script's poll.</summary>
    public void Poll() => Run("poll()");

    /// <summary>Opens the bag screen's loot view, changed from the default by the given statements on its state `s`, and lets the poll find it.</summary>
    public void OpenLoot(string changes = "")
    {
        Run($"(function () {{ var s = lootState(); {changes} openBag(s); }})()");
        Poll();
    }

    /// <summary>Changes the open screen's state `s`, as the game's next message to the page would, and lets the poll see it.</summary>
    public void Change(string changes)
    {
        Run($"(function () {{ var s = bagState(); {changes} }})()");
        Poll();
    }

    public void Press(string element) => Run($"press({element})");

    /// <summary>The plugin sending the script a second time, which must find the first copy already running.</summary>
    public void SendScriptAgain() => Run(Resource("TrunkAnywhere.page.js"));

    /// <summary>Takes what the script posted since the last look.</summary>
    private void Hear()
    {
        var count = (int)_engine.Evaluate("posted.length").AsNumber();
        var messages = Enumerable.Range(0, count).Select(i => _engine.Evaluate($"posted[{i}]").AsString()).ToList();

        _engine.Execute("posted.length = 0");
        Posted.AddRange(messages.Select(PageMessage.Parse));
    }

    private static string Resource(string name)
    {
        using var stream = typeof(PageScript).Assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException(name + " is not embedded in the test assembly");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
