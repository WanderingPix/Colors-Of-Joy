using System.Text.Json;
using System.Text.Json.Serialization;
using AmongUs.GameOptions;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using ColorsOfJoy.Networking;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using InnerNet;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorsOfJoy.Patches;
 
[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void PlayerControl_Start_Postfix(PlayerControl __instance)
    {
        var playerColorData = __instance.gameObject.AddComponent<PlayerColorData>();
        playerColorData.colorblindText = __instance.cosmetics.colorBlindText;
        if (__instance.AmOwner)
        {
            __instance.StartCoroutine(Effects.ActionAfterDelay(0.5f, new System.Action(() =>
            {
                if (ColorsOfJoyPlugin.LastSetColor.Value > CustomColorsDataManager.Colors.Count - 1)
                    ColorsOfJoyPlugin.LastSetColor.Value = 0;
                var color = CustomColorsDataManager.Colors[ColorsOfJoyPlugin.LastSetColor.Value];
                __instance.RpcSetColor(JsonSerializer.Serialize(color, new JsonSerializerOptions()
                {
                    Converters = { new Color32JsonConverter() },
                    IncludeFields = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                }));
            })));
        }
        else
            __instance.StartCoroutine(Effects.ActionAfterDelay(0.5f, new System.Action(() =>
            {
                Rpc<RpcSyncColors>.Instance.SendTo(PlayerControl.LocalPlayer, __instance.Data.ClientId,
                    JsonSerializer.Serialize(PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color,
                        new JsonSerializerOptions()
                        {
                            Converters = { new Color32JsonConverter() },
                            IncludeFields = true,
                            ReferenceHandler = ReferenceHandler.IgnoreCycles
                        }));
            })));
    }
    
    [HarmonyPatch(nameof(PlayerControl.RawSetHat))]
    [HarmonyPatch(nameof(PlayerControl.RawSetOutfit))]
    [HarmonyPatch(nameof(PlayerControl.RawSetSkin))]
    [HarmonyPatch(nameof(PlayerControl.RawSetVisor))]
    [HarmonyPatch(nameof(PlayerControl.RawSetPet))]
    [HarmonyPostfix]
    public static void PlayerControl_RawSetCosmetics_Postfix(PlayerControl __instance)
    {
        var playerColorData = __instance.GetComponent<PlayerColorData>();
        if (playerColorData == null || playerColorData.colorblindText == null) return;
        playerColorData.SetColor(playerColorData.Color);
    }
    
    [HarmonyPatch(nameof(PlayerControl.HandleRpc))]
    [HarmonyPrefix]
    public static bool PlayerControl_HandleRpc_Prefix(PlayerControl __instance, ref byte callId, ref MessageReader reader)
    {
        if ((RpcCalls)callId == RpcCalls.Shapeshift)
        {
            CustomShapeshift(__instance, reader.ReadNetObject<PlayerControl>(), reader.ReadBoolean());
            return false;
        }

        return true;
    }

    public static void CustomShapeshift(this PlayerControl source, PlayerControl targetPlayer, bool animate)
    {
        source.waitingForShapeshiftResponse = false;
        if (source.CurrentOutfitType == PlayerOutfitType.MushroomMixup)
        {
            source.logger.Info($"Ignoring shapeshift message for {((UnityEngine.Object) targetPlayer == (UnityEngine.Object) null ? "null player" : targetPlayer.PlayerId.ToString())} because of mushroom mixup");
            if (!source.AmOwner)
                return;
            DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(source.Data.Role.Ability);
            source.Data.Role.SetCooldown();
        }
        else
        {
            NetworkedPlayerInfo targetPlayerInfo = targetPlayer.Data;
            NetworkedPlayerInfo.PlayerOutfit newOutfit = (int) targetPlayerInfo.PlayerId != (int) source.Data.PlayerId ? targetPlayer.Data.Outfits[PlayerOutfitType.Default] : source.Data.Outfits[PlayerOutfitType.Default];
            var sourceColorData = source.GetComponent<PlayerColorData>();
            var targetColorData = targetPlayer.GetComponent<PlayerColorData>();
            System.Action changeOutfit = (System.Action) (() =>
            {
                if ((int) targetPlayerInfo.PlayerId == (int) source.Data.PlayerId)
                {
                    source.RawSetOutfit(newOutfit, PlayerOutfitType.Default);
                    source.StartCoroutine(Effects.ActionAfterDelay(0.1f, new System.Action(() => sourceColorData.SetColor(sourceColorData.Color))));
                    source.logger.Info( $"Player {source.PlayerId} Shapeshift is reverting");
                    source.shapeshiftTargetPlayerId = -1;
                    if (!source.AmOwner)
                        return;
                    DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(source.Data.Role.Ability);
                }
                else
                {
                    source.RawSetOutfit(newOutfit, PlayerOutfitType.Shapeshifted);
                    source.logger.Info($"Player {source.PlayerId} is shapeshifting into {targetPlayer.PlayerId}");
                    source.StartCoroutine(Effects.ActionAfterDelay(0.1f, new System.Action(() => sourceColorData.SetColorAsShapeshifter(targetColorData.Color))));
                    source.shapeshiftTargetPlayerId = (int) targetPlayer.PlayerId;
                    if (!source.AmOwner)
                        return;
                    DestroyableSingleton<HudManager>.Instance.AbilityButton.OverrideText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShapeshiftAbilityUndo));
                }
            });
            if (animate)
            {
                source.shapeshifting = true;
                source.MyPhysics.SetNormalizedVelocity(Vector2.zero);
                if (source.AmOwner && !(bool) (UnityEngine.Object) Minigame.Instance)
                    PlayerControl.HideCursorTemporarily();
                RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, source.gameObject.transform);
                roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(source.AmOwner);
                PlayerMaterialExtensions.SetColors(sourceColorData.Color, roleEffectAnimation.Renderer.material);
                if (source.cosmetics.FlipX)
                    roleEffectAnimation.transform.position -= new Vector3(0.14f, 0.0f, 0.0f);
                roleEffectAnimation.MidAnimCB = (System.Action) (() =>
                {
                    changeOutfit();
                    source.cosmetics.SetScale(source.MyPhysics.Animations.DefaultPlayerScale, source.defaultCosmeticsScale);
                    source.Data.Role.TryCast<ShapeshifterRole>()?.SetEvidence();
                });
                float shapeshiftScale = source.MyPhysics.Animations.ShapeshiftScale;
                if (AprilFoolsMode.ShouldLongAround())
                {
                    source.cosmetics.ShowLongModeParts(false);
                    source.cosmetics.SetHatVisorVisible(false);
                }
                source.StartCoroutine(source.ScalePlayer(shapeshiftScale, 0.25f));
                roleEffectAnimation.Play(source, (System.Action) (() =>
                {
                    source.shapeshifting = false;
                    if (!AprilFoolsMode.ShouldLongAround())
                        return;
                    source.cosmetics.ShowLongModeParts(true);
                    source.cosmetics.SetHatVisorVisible(true);
                }), PlayerControl.LocalPlayer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local);
            }
            else
                changeOutfit();
        }
    }
    
    [HarmonyPatch(nameof(PlayerControl.CheckShapeshift))]
    [HarmonyPrefix]
    private static void CheckShapeshift(PlayerControl __instance, ref PlayerControl target, ref bool shouldAnimate)
    {
        __instance.logger.Debug($"Checking if {__instance.PlayerId} can shapeshift into {(target == null ? "null player" : (object) target.PlayerId.ToString())}");
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
            return;
        if (__instance.AmOwner) __instance.CustomShapeshift(target, shouldAnimate);
        if (!(bool) (Object) target || target.Data == null || __instance.Data.IsDead || __instance.Data.Role.Role != RoleTypes.Shapeshifter || __instance.Data.Disconnected)
        {
            __instance.logger.Warning($"Bad shapeshift from {__instance.PlayerId} to {((bool) (Object) target ? target.PlayerId : -1)}");
            __instance.RpcRejectShapeshift();
        }
        else if (target.IsMushroomMixupActive() & shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while mushroom mixup was active");
            __instance.RpcRejectShapeshift();
        }
        else if ((bool) (Object) MeetingHud.Instance & shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while a meeting was starting");
            __instance.RpcRejectShapeshift();
        }
        else
            __instance.RpcCustomShapeshift(target, shouldAnimate);
    }
}
 
