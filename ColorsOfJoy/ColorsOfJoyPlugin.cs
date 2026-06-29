using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace ColorsOfJoy;

[BepInAutoPlugin]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class ColorsOfJoyPlugin : BasePlugin
{
    public static ConfigEntry<int> LastSetColor;
    public Harmony Harmony { get; } = new(Id);
    public override void Load()
    {
        Harmony.PatchAll();
        LastSetColor = Config.Bind("Data", "Last Set Color Index", 0);
        CustomColorsDataManager.LoadData();
    }
}