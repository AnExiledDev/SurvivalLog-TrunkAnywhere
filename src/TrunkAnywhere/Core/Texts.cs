using System.Collections.Generic;
using ExiledToolkit.Kit;

namespace TrunkAnywhere.Core;

/// <summary>The words the mod puts on screen.</summary>
public static class Texts
{
    public static readonly Phrase TakeAllLabel = new("Take All to Trunk", "全部放进后备箱");

    public static readonly Phrase Blocked = new("Pay for it first: unpaid items can't be moved into the trunk.", "请先付款：未付款的物品不能移进后备箱。");

    public static IReadOnlyList<Phrase> All { get; } = new[] { TakeAllLabel, Blocked };
}
