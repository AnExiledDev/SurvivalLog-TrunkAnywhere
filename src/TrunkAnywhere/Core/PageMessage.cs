using System;

namespace TrunkAnywhere.Core;

/// <summary>
/// What the page script tells the plugin. The script sends "slmod|trunkanywhere|" followed by a verb and its argument,
/// split by '|'; anything else parses to Unknown so it can be logged rather than acted on.
/// </summary>
public abstract record PageMessage
{
    public const string Prefix = "slmod|trunkanywhere|";

    private PageMessage()
    {
    }

    /// <summary>The Take All to Trunk button was pressed.</summary>
    public sealed record TakeAllToTrunk : PageMessage;

    /// <summary>Something on the page failed in a way the plugin's log should show; each kind is sent once.</summary>
    public sealed record Diag(string Text) : PageMessage;

    public sealed record Unknown(string Text) : PageMessage;

    public static bool IsOurs(string? message) => message is not null && message.StartsWith(Prefix, StringComparison.Ordinal);

    /// <summary>Parses a whole message, prefix included.</summary>
    public static PageMessage Parse(string message)
    {
        if (!IsOurs(message)) return new Unknown(message);

        var body = message.Substring(Prefix.Length);
        var split = body.IndexOf('|');
        var verb = split < 0 ? body : body.Substring(0, split);

        return verb switch
        {
            "takeAllToTrunk" when split < 0 => new TakeAllToTrunk(),
            "diag" when split >= 0 => new Diag(body.Substring(split + 1)),
            _ => new Unknown(message),
        };
    }
}
