using System;
using System.Collections.Concurrent;
using HarmonyLib;
using TrunkAnywhere.Core;
using Vuplex.WebView.Internal;

namespace TrunkAnywhere.Adapters;

/// <summary>
/// Catches the page script's messages before the game's web layer sees them (it would log them as unknown) and queues
/// them for the session's next frame. Other mods hook the same method for their own prefix; a message that is not ours
/// is always handed on. Static because a Harmony prefix has nowhere else to look.
/// </summary>
internal static class PageMessages
{
    private static readonly ConcurrentQueue<PageMessage> Queue = new();
    private static Action<string> _log = _ => { };
    private static bool _hasReportedFailure;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            harmony.Patch(AccessTools.Method(typeof(BaseWebView), "HandleMessageEmitted"), prefix: new HarmonyMethod(typeof(PageMessages), nameof(TakeOurs)));
            log("listening for the bag screen's Take All to Trunk button");
        }
        catch (Exception ex)
        {
            log("could not listen for page messages, the Take All to Trunk button will do nothing: " + ex.Message);
        }
    }

    public static bool TryTake(out PageMessage message)
    {
        if (Queue.TryDequeue(out var queued))
        {
            message = queued;

            return true;
        }

        message = new PageMessage.Unknown("");

        return false;
    }

    /// <summary>A throw here would break every web message in the game, so any failure lets the game handle the message.</summary>
    private static bool TakeOurs(string serializedMessage)
    {
        try
        {
            if (!PageMessage.IsOurs(serializedMessage)) return true;

            Queue.Enqueue(PageMessage.Parse(serializedMessage));

            return false;
        }
        catch (Exception ex)
        {
            ReportFailureOnce(ex);

            return true;
        }
    }

    private static void ReportFailureOnce(Exception ex)
    {
        if (_hasReportedFailure) return;

        _hasReportedFailure = true;

        _log("reading a page message failed, the game handles it instead: " + ex.Message);
    }
}
