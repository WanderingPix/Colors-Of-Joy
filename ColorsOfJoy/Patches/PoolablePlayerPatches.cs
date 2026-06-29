using System;
using System.Linq;
using ColorsOfJoy.Components;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorsOfJoy.Patches;
 
[HarmonyPatch(typeof(PoolablePlayer))]
public class PoolablePlayerPatch
{
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromPlayerData))]
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
    [HarmonyPostfix]
    public static void PoolablePlayer_UpdateFromPlayerData_Postfix(PoolablePlayer __instance, ref NetworkedPlayerInfo pData)
    {
        var extendedPoolablePlayer = __instance.GetComponent<ExtendedPoolablePlayer>();
        extendedPoolablePlayer.owner = pData.Object;
        if (__instance.transform.parent.TryGetComponent(out ChatBubble bubble)) extendedPoolablePlayer.bubble = bubble;
        if (__instance.transform.parent.TryGetComponent(out PlayerVoteArea area)) extendedPoolablePlayer.voteArea = area;
    }
    [HarmonyPatch(nameof(PoolablePlayer.UpdateFromLocalPlayer))]
    [HarmonyPostfix]
    public static void PoolablePlayer_UpdateFromLocalPlayer_Postfix(PoolablePlayer __instance)
    {
        if (!__instance.TryGetComponent(out ExtendedPoolablePlayer p)) return;
        p.owner = PlayerControl.LocalPlayer;
    }
    [HarmonyPatch(nameof(PoolablePlayer.Awake))]
    [HarmonyPostfix]
    public static void PoolablePlayer_Awake_Postfix(PoolablePlayer __instance)
    {
        __instance.gameObject.AddComponent<ExtendedPoolablePlayer>();
    }
}
 
