using UnityEngine;

namespace ColorsOfJoy;

public static class PlayerMaterialExtensions
{
    public static void SetColors(CustomPlayerColor color, Material material)
    {
        if (color == null || material == null) return;
        material.SetColor(PlayerMaterial.BackColor, color.SecondaryColor);
        material.SetColor(PlayerMaterial.BodyColor, color.MainColor);
        material.SetColor(PlayerMaterial.VisorColor, color.VisorColor);
    }
}