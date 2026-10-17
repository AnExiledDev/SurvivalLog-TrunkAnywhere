using System.Collections.Generic;

namespace TrunkAnywhere.Core;

/// <summary>One piece of on-screen text in the two languages the game ships.</summary>
public readonly record struct Phrase(string En, string Zh)
{
    public string Pick(bool isChinese) => isChinese ? Zh : En;
}

/// <summary>The words the mod puts on screen.</summary>
public static class Texts
{
    public static readonly Phrase TakeAllLabel = new("Take All to Trunk", "全部放进后备箱");

    public static readonly Phrase Blocked = new("Pay for it first: unpaid items can't go in the trunk.", "请先付款：未付款的物品不能放进后备箱。");

    public static readonly Phrase BuyBlocked = new("Buy into your bag: unpaid items can't go in the trunk.", "请买进背包：未付款的物品不能放进后备箱。");

    public static IReadOnlyList<Phrase> All { get; } = new[] { TakeAllLabel, Blocked, BuyBlocked };
}
