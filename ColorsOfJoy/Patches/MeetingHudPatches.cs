using System.Linq;
using ColorsOfJoy.Components;
using HarmonyLib;
using UnityEngine;

namespace ColorsOfJoy.Patches;

[HarmonyPatch]
public class MeetingHudPatches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPostfix]
    public static void Begin(ExileController __instance, ref ExileController.InitProperties init)
    {
        __instance.Player.UpdateFromPlayerData(init.networkedPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, true, null, true);
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void Start(MeetingHud __instance)
    {
        foreach (var rend in __instance.PlayerColoredParts)
        {
            PlayerMaterialExtensions.SetColors(PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color, rend.material);
        }
    }
    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
    [HarmonyPostfix]
    public static void Start(MeetingCalledAnimation __instance, ref NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        var playerOutfit = outfit;
        var color = PlayerControl.AllPlayerControls.ToArray().First(x => x.CurrentOutfit == playerOutfit).GetComponent<PlayerColorData>().Color;
        foreach (var rend in __instance.playerParts.Cosmetics.bodySprites)
        {
            PlayerMaterialExtensions.SetColors(color, rend.BodySprite.material);
        }
        foreach (var rend in __instance.classicPlayerParts.Cosmetics.bodySprites)
        {
            PlayerMaterialExtensions.SetColors(color, rend.BodySprite.material);
        }
    }
    
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
    [HarmonyPostfix]
    public static void BloopAVoteIcon(MeetingHud __instance, ref NetworkedPlayerInfo voterPlayer, ref int index, ref Transform parent)
    {
        SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab, parent, true);
        if (GameManager.Instance.LogicOptions.GetAnonymousVotes())
            PlayerMaterial.SetColors(Palette.DisabledGrey, (Renderer) spriteRenderer);
        else
            PlayerMaterialExtensions.SetColors(voterPlayer.Object.GetComponent<PlayerColorData>().Color, spriteRenderer.material);
        spriteRenderer.transform.localScale = Vector3.zero;
        PlayerVoteArea component = parent.GetComponent<PlayerVoteArea>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
        __instance.StartCoroutine(Effects.Bloop((float) index * 0.3f, spriteRenderer.transform));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
    }
}