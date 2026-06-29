using System;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace ColorsOfJoy.Components;

[RegisterInIl2Cpp]
public class PlayerColorData(IntPtr ptr) : MonoBehaviour(ptr)
{
    public CustomPlayerColor Color;
    public CustomPlayerColor ShapeshiftedColor;
    public TextMeshPro colorblindText;
    public void SetColor(CustomPlayerColor color)
    {
        Color = color;
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
        {
            PlayerMaterialExtensions.SetColors(color, r.material);
        }
        colorblindText.text = color.Name;
    }
    public void SetColorAsShapeshifter(CustomPlayerColor color)
    {
        ShapeshiftedColor = color;
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
        {
            PlayerMaterialExtensions.SetColors(color, r.material);
        }
        colorblindText.text = color.Name;
    }
}