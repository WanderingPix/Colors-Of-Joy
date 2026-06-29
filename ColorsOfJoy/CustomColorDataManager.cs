using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using UnityEngine;

namespace ColorsOfJoy;

public class CustomColorsDataManager
{
    public static List<CustomPlayerColor> Colors = new();
    public static readonly List<CustomPlayerColor> DefaultColors =
    [
        new CustomPlayerColor(new Color32(198, 17, 17, 255),  "Red",     new Color32(122, 8, 8, 255),    new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(19, 46, 210, 255),  "Blue",    new Color32(12, 28, 121, 255),  new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(17, 127, 45, 255),  "Green",   new Color32(10, 77, 28, 255),   new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(238, 84, 187, 255), "Pink",    new Color32(172, 55, 140, 255), new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(240, 125, 13, 255), "Orange",  new Color32(180, 90, 10, 255),  new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(246, 246, 87, 255), "Yellow",  new Color32(190, 190, 40, 255), new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(63, 71, 78, 255),   "Black",   new Color32(37, 40, 45, 255),   new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(215, 225, 235, 255),"White",   new Color32(173, 180, 192, 255),new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(107, 67, 188, 255), "Purple",  new Color32(63, 38, 109, 255),  new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(113, 73, 30, 255),  "Brown",   new Color32(75, 45, 17, 255),   new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(56, 255, 219, 255), "Cyan",    new Color32(30, 168, 144, 255), new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(80, 240, 57, 255),  "Lime",    new Color32(46, 142, 29, 255),  new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(132, 19, 72, 255),  "Maroon",  new Color32(92, 15, 51, 255),   new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(213, 156, 173, 255),"Rose",    new Color32(154, 99, 111, 255), new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(245, 245, 161, 255),"Banana",  new Color32(178, 173, 95, 255), new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(140, 140, 140, 255),"Gray",    new Color32(95, 95, 95, 255),   new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(189, 150, 115, 255),"Tan",     new Color32(133, 99, 73, 255),  new Color32(142, 206, 251, 255)),
        new CustomPlayerColor(new Color32(255, 103, 86, 255), "Coral",   new Color32(180, 62, 48, 255),  new Color32(142, 206, 251, 255)),
    ];
    public static void LoadData()
    {
        Colors.Clear();
        Colors.AddRange(DefaultColors);
        if (!Directory.Exists(GetPath())) 
            Directory.CreateDirectory(GetPath());
        foreach (var file in Directory.GetFiles(GetPath()))
        {
            if (!file.EndsWith(".json")) continue;
            var json = File.ReadAllText(Path.Combine(GetPath(), file));
            var data = JsonSerializer.Deserialize<CustomPlayerColor>(json, new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                IncludeFields = true,
                Converters = { new Color32JsonConverter() }
            });
            Colors.Add(data);
        }
    }
    public static void Save(CustomPlayerColor color)
    {
        var text = JsonSerializer.Serialize(color, new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            IncludeFields = true,
            Converters = { new Color32JsonConverter() }
        });
        Colors.Add(color);
        File.WriteAllText(Path.Combine(GetPath(), color.Name + ".json"), text);
    }

    public static string GetPath()
    {
        if (OperatingSystem.IsAndroid()) return Environment.GetEnvironmentVariable("STAR_DATA_PATH");
        return Path.Combine(Paths.GameRootPath, "ColorsOfJoy");
    }
}