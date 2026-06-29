using System;
using System.Linq;
using ColorsOfJoy.Components;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorsOfJoy.Patches;
 
[HarmonyPatch(typeof(RoleEffectAnimation))]
public class RoleEffectAnimationPatch
{
    [HarmonyPatch(nameof(RoleEffectAnimation.Play))]
    [HarmonyPrefix]
    public static void PoolablePlayer_Play_Postfix(RoleEffectAnimation __instance, ref PlayerControl parent)
    {
        PlayerMaterialExtensions.SetColors(parent.GetComponent<PlayerColorData>().Color, __instance.Renderer.material);
    }
}
 
