using System;
using System.Text.Json.Serialization;
using ColorsOfJoy.Components;
using UnityEngine;

namespace ColorsOfJoy;

[JsonSerializable(typeof(PlayerColorData))]
[Serializable]
public class CustomPlayerColor(Color32 mainColor, string name, Color32 secondaryColor, Color32 visorColor)
{
    public string Name { get; set; } = name;
    public Color32 MainColor { get; set; } = mainColor;
    public Color32 SecondaryColor { get; set; } = secondaryColor;
    public Color32 VisorColor { get; set; } = visorColor;
}

public enum PlayerColorTypes
{
    Main,
    Shadow,
    Visor
}