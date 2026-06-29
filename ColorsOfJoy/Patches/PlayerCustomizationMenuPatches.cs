using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using ColorsOfJoy.Networking;
using HarmonyLib;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ColorsOfJoy.Patches;
 
[HarmonyPatch(typeof(PlayerTab))]
public class PlayerTabPatches
{
    [HarmonyPatch(nameof(PlayerTab.UpdateAvailableColors))]
    [HarmonyPostfix]
    public static void PlayerTab_UpdateAvailableColors_Postfix(PlayerTab __instance)
    {
        __instance.AvailableColors.Clear();
        for (int i = 0; i < __instance.ColorChips.Count; i++)
        {
            __instance.AvailableColors.Add(i);
        }
    }
    
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Update))]
    [HarmonyPostfix]
    public static void PlayerCustomizationMenu_UpdateAvailableColors_Postfix(PlayerCustomizationMenu __instance)
    {
        if (__instance.transform.FindChild("ColorGroup").gameObject.active) return;
        if (PlayerControl.LocalPlayer == null) return;
        foreach (var rend in __instance.PreviewArea.GetComponentsInChildren<SpriteRenderer>())
        {
            PlayerMaterialExtensions.SetColors(PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color, rend.material);
        }
    }
    
    [HarmonyPatch(nameof(PlayerTab.Update))]
    [HarmonyPostfix]
    public static void PlayerTab_Update_Postfix(PlayerTab __instance)
    {
        PlayerCustomizationMenu.Instance.equipButton.SetActive(false);
    }

    [HarmonyPatch(nameof(PlayerTab.ClickEquip))]
    [HarmonyPrefix]
    public static bool PlayerTab_ClickEquip_Postfix(PlayerTab __instance)
    {
        var color = CustomColorsDataManager.Colors[__instance.currentColor];
        var json = JsonSerializer.Serialize(color, new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new Color32JsonConverter() },
            IncludeFields = true
        });
        if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.RpcSetColor(json);
        return false;
    }

    private static GameObject ColorCreationUI;
    private static CustomPlayerColor currentlyBeingCreatedColor = new(mainColor: Palette.PlayerColors[0],
        name: "New Color", secondaryColor: Palette.ShadowColors[0], visorColor: Palette.VisorColor);
    
    private static BoxCollider2D? _collider;
    [HarmonyPatch(nameof(PlayerTab.OnEnable))]
    [HarmonyPostfix]
    public static void PlayerTab_OnEnable_Postfix(PlayerTab __instance)
    {
        SetUpScroller(__instance);
        __instance.PlayerPreview.GetComponent<ExtendedPoolablePlayer>().Destroy();
        foreach (var existingChip in __instance.ColorChips)
        {
            existingChip.gameObject.Destroy();
        }

        __instance.ColorChips = new();
        foreach (var col in CustomColorsDataManager.Colors)
        {
            CreateColorChip(__instance, col).Button.OnClick.Invoke();
        }
        
        if (!PlayerCustomizationMenu.Instance)
        {
            return;
        }

        if (PlayerControl.LocalPlayer == null || HudManager.Instance == null) return; //No color creation in main menu.
        if (ColorCreationUI != null) return;
        UpdatePreview(__instance, PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color);
        ColorCreationUI = new GameObject("ColorCreationUI");
        ColorCreationUI.transform.SetParent(__instance.transform);
        ColorCreationUI.transform.position = __instance.transform.position;
        ColorCreationUI.SetActive(false);
        var buttonPrefab = Object.FindObjectOfType<OptionsMenuBehaviour>(true).EnableFriendInvitesButton
            .GetComponent<PassiveButton>();
        var newColorButton = Object.Instantiate(buttonPrefab, __instance.transform.GetChild(0));
        newColorButton.GetComponent<ToggleButtonBehaviour>().Destroy();
        newColorButton.transform.localPosition = new Vector3(0.25f, 0, -10);
        newColorButton.transform.SetParent(__instance.transform, true);
        newColorButton.GetComponentInChildren<TextMeshPro>().text = "New Color";
        newColorButton.OnClick = new();
        newColorButton.OnClick.AddListener(new Action(() =>
        {
            bool active = !ColorCreationUI.gameObject.active;
            ColorCreationUI.gameObject.SetActive(!ColorCreationUI.gameObject.active);
            foreach (var chip in __instance.ColorChips)
            {
                chip.gameObject.SetActive(!active);
            }
            UpdatePreview(__instance);
        }));

        CreateMenu(__instance.transform, new Vector3(-4, 1, -10), () => currentlyBeingCreatedColor.MainColor, __instance, PlayerColorTypes.Main);
        CreateMenu(__instance.transform, new Vector3(-1.5f, 1, -10), () => currentlyBeingCreatedColor.SecondaryColor, __instance, PlayerColorTypes.Shadow);
        CreateMenu(__instance.transform, new Vector3(-2.75f, -1, -10), () => currentlyBeingCreatedColor.VisorColor, __instance, PlayerColorTypes.Visor);
        var saveButton = Object.Instantiate(buttonPrefab, ColorCreationUI.transform);
        saveButton.GetComponent<ToggleButtonBehaviour>().Destroy();
        saveButton.transform.localPosition = new Vector3(-1f, -2.3f, -50);
        saveButton.transform.localScale = Vector3.one * 0.8f;
        saveButton.GetComponentInChildren<TextMeshPro>().text = "Save";
        saveButton.OnClick = new Button.ButtonClickedEvent();
        saveButton.OnClick.AddListener(new Action(() =>
        {
            ColorCreationUI.gameObject.SetActive(false);
            foreach (var chip in __instance.ColorChips)
            {
                chip.gameObject.SetActive(true);
            }
            UpdatePreview(__instance, PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color);
            CustomColorsDataManager.Save(currentlyBeingCreatedColor);
            CreateColorChip(__instance, currentlyBeingCreatedColor).Button.OnClick.Invoke();
        }));
        var cancelButton = Object.Instantiate(buttonPrefab, ColorCreationUI.transform);
        cancelButton.GetComponent<ToggleButtonBehaviour>().Destroy();
        cancelButton.transform.localPosition = new Vector3(1f, -2.3f, -50);
        cancelButton.transform.localScale = Vector3.one * 0.8f;
        cancelButton.GetComponentInChildren<TextMeshPro>().text = "Cancel";
        cancelButton.OnClick = new Button.ButtonClickedEvent();
        cancelButton.OnClick.AddListener(new Action(() =>
        {
            ColorCreationUI.gameObject.SetActive(false);
            foreach (var chip in __instance.ColorChips)
            {
                chip.gameObject.SetActive(true);
            }
            UpdatePreview(__instance, PlayerControl.LocalPlayer.GetComponent<PlayerColorData>().Color);
        }));
        var openDirectoryButton = Object.Instantiate(newColorButton, newColorButton.transform.parent);
        openDirectoryButton.GetComponentInChildren<TextMeshPro>().text = "";
        openDirectoryButton.GetComponent<BoxCollider2D>().size = Vector2.one;
        var dirSpriteRenderer = openDirectoryButton.transform.GetChild(0).GetComponent<SpriteRenderer>();
        dirSpriteRenderer.sprite =
            SpriteTools.LoadSpriteFromPath("ColorsOfJoy.Resources.FolderIcon.png",
                Assembly.GetAssembly(typeof(PlayerTabPatches)), 256);
        dirSpriteRenderer.size = Vector2.one;
        openDirectoryButton.transform.GetChild(1).gameObject.SetActive(false);
        openDirectoryButton.OnClick = new Button.ButtonClickedEvent();
        openDirectoryButton.transform.position = newColorButton.transform.position + new Vector3(2, 0, 0);
        openDirectoryButton.OnClick = new Button.ButtonClickedEvent();
        openDirectoryButton.OnClick.AddListener(new System.Action(() =>
        {
            Process.Start("explorer.exe", CustomColorsDataManager.GetPath());
        }));
        var refreshButton = Object.Instantiate(openDirectoryButton, newColorButton.transform.parent);
        var refreshSpriteRenderer = refreshButton.transform.GetChild(0).GetComponent<SpriteRenderer>();
        refreshSpriteRenderer.sprite =
            SpriteTools.LoadSpriteFromPath("ColorsOfJoy.Resources.RefreshIcon.png",
                Assembly.GetAssembly(typeof(PlayerTabPatches)), 256);
        refreshSpriteRenderer.size = Vector2.one;
        refreshButton.OnClick = new Button.ButtonClickedEvent();
        refreshButton.transform.localPosition += new Vector3(1, 0, 0);
        refreshButton.OnClick.AddListener(new System.Action(() =>
        {
            CustomColorsDataManager.LoadData();
            __instance.OnDisable();
            __instance.OnEnable();
        }));
        var nameField = Object.Instantiate(HudManager.Instance.Chat.freeChatField);
        nameField.submitButton.gameObject.SetActive(false);
        nameField.SetCanSubmit(false);
        nameField.transform.position = PlayerCustomizationMenu.Instance.itemName.transform.position - new Vector3(0, 0, 50);
        // ReSharper disable once Unity.InstantiateWithoutParent
        nameField.transform.SetParent(ColorCreationUI.transform, true);
        Vector3 pos = nameField.transform.position;
        nameField.transform.localScale = Vector3.one / 2f;
        nameField.OnChangedEvent = new Action(() =>
        {
            currentlyBeingCreatedColor.Name = nameField.Text;
            nameField.transform.position = pos;
        });
        __instance.SetScrollerBounds();
        __instance.StartCoroutine(Effects.ActionAfterDelay(0.1f, new System.Action(() => nameField.transform.position = pos)));
    }

    private static void SetUpScroller(PlayerTab playerTab)
    {
        if (playerTab.scroller == null)
        {
            var tab = PlayerCustomizationMenu.Instance.Tabs[1].Tab;
            var newScroller = Object.Instantiate(tab.scroller, playerTab.transform, true);
            newScroller.transform.position = playerTab.ColorTabArea.position;
            newScroller.Inner.transform.DestroyChildren();

            _collider = playerTab.gameObject.AddComponent<BoxCollider2D>();
            _collider?.size = new Vector2(1f, 0.75f);
            _collider?.enabled = true;
            playerTab.scroller = newScroller;
        }

        playerTab.SetScrollerBounds();
    }
 
    private static SlideBar CreateSlider(string title, Action<float> OnValueChanged)
    {
        var prefab = Object.FindObjectOfType<OptionsMenuBehaviour>(true).SoundSlider;
        var slider = Object.Instantiate(prefab);
        slider.Range = new(-0.75f, 0.75f);
        slider.SetValue(0);
        slider.Title = slider.transform.GetChild(1).GetComponent<TextMeshPro>();
        slider.Title.GetComponent<TextTranslatorTMP>().Destroy();
        slider.Title.text = title;
        slider.Vertical = true;
        slider.Bar.size = new Vector2(0.05f, 1.5f);
        var sliderCol = slider.Bar.GetComponent<BoxCollider2D>();
        sliderCol.size = new Vector2(0.05f, 3f);
        sliderCol.offset = new Vector2(0, 0f);
        if (OperatingSystem.IsAndroid()) sliderCol.size *= 1.5f;
        slider.Dot.transform.localPosition = new Vector3(0, slider.Dot.transform.localPosition.y, -10);
        slider.Title.transform.position = slider.Bar.transform.position - new Vector3(0, 1);
        slider.OnValueChange = new();
        slider.OnValueChange.AddListener(new Action(() => OnValueChanged.Invoke(slider.Value)));
 
        return slider;
    }
 
    private static void CreateMenu(Transform parent, Vector3 position, Func<Color32> getColor, PlayerTab __instance, PlayerColorTypes type)
    {
        var transform = new GameObject("ColorControls").transform;
        transform.SetParent(parent);
        transform.localPosition = position;
        transform.localScale = Vector3.one * 0.7f;
 
        var redSlider = CreateSlider("R", r =>
        {
            var current = getColor.Invoke();
            SetColor(new Color32((byte)(r * 255), current.g, current.b, 255), type);
            UpdatePreview(__instance);
        });
        redSlider.transform.SetParent(transform);
        redSlider.transform.localPosition = new Vector3(-0.75f, 0, -10);
        redSlider.transform.SetParent(ColorCreationUI.transform, true);
        var greenSlider = CreateSlider("G", g =>
        {
            var current = getColor.Invoke();
            SetColor(new Color32(current.r, (byte)(g * 255), current.b, 255), type);
            UpdatePreview(__instance);
        });
        greenSlider.transform.SetParent(transform);
        greenSlider.transform.localPosition = new Vector3(0f, 0, -10);
        greenSlider.transform.SetParent(ColorCreationUI.transform, true);
        var blueSlider = CreateSlider("B", b =>
        {
            var current = getColor.Invoke();
            SetColor(new Color32(current.r, current.g, (byte)(b * 255), 255), type);
            UpdatePreview(__instance);
        });
        blueSlider.transform.SetParent(transform);
        blueSlider.transform.localPosition = new Vector3(0.75f, 0, -10);
        blueSlider.transform.SetParent(ColorCreationUI.transform, true);
    }

    private static void UpdatePreview(PlayerTab tab, CustomPlayerColor color = null)
    {
        if (color == null) color = currentlyBeingCreatedColor;
        foreach (var rend in tab.PlayerPreview.GetComponentsInChildren<SpriteRenderer>())
        {
            PlayerMaterialExtensions.SetColors(color, rend.material);
        }
        PlayerCustomizationMenu.Instance.SetItemName(color.Name);
    }

    private static void SetColor(Color32 color, PlayerColorTypes type)
    {
        switch (type)
        {
            case PlayerColorTypes.Main:
                currentlyBeingCreatedColor.MainColor = color;
                break;
            case PlayerColorTypes.Shadow:
                currentlyBeingCreatedColor.SecondaryColor = color;
                break;
            case PlayerColorTypes.Visor:
                currentlyBeingCreatedColor.VisorColor = color;
                break;
        }
    }

    private static ColorChip CreateColorChip(PlayerTab tab, CustomPlayerColor color)
    {
        int index = tab.ColorChips.Count;
        float x = tab.XRange.Lerp((float) (index % 4) / 3f);
        float y = tab.YStart - (float) (index / 4f) * tab.YOffset;
        ColorChip colorChip = Object.Instantiate<ColorChip>(tab.ColorTabPrefab, tab.scroller.Inner, true);
        colorChip.transform.localPosition = new Vector3(x, y, -1f);
        colorChip.Inner.SpriteColor = color.MainColor;
        tab.ColorChips.Add(colorChip);
        if (true)
        {
            colorChip.Button.OnClick = new Button.ButtonClickedEvent();
            colorChip.Button.OnClick.AddListener(new Action(() =>
            {
                var json = JsonSerializer.Serialize(color, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Converters = { new Color32JsonConverter() },
                    IncludeFields = true
                });
                if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.RpcSetColor(json);
                UpdatePreview(tab, color);
                tab.currentColor = tab.ColorChips.IndexOf(colorChip);
                ColorsOfJoyPlugin.LastSetColor.Value = tab.ColorChips.IndexOf(colorChip);
            }));
            colorChip.Button.OnMouseOver.AddListener(new Action(() =>
            {
                PlayerCustomizationMenu.Instance.SetItemName(color.Name);
                UpdatePreview(tab, color);
            }));
            if (PlayerControl.LocalPlayer) colorChip.Button.OnMouseOut.AddListener(new Action(() =>
            {
                if (!PlayerControl.LocalPlayer) return;
                var playerColorData = PlayerControl.LocalPlayer
                    .GetComponent<PlayerColorData>();
                PlayerCustomizationMenu.Instance.SetItemName(playerColorData.Color.Name);
                UpdatePreview(tab, playerColorData.Color);
            }));
        }

        return colorChip;
    }
}