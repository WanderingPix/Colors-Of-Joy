using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Unity.IL2CPP;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace ColorsOfJoy.Networking;

[RegisterCustomRpc((uint)CojRpcCalls.SyncColors)]
internal sealed class RpcSyncColors : PlayerCustomRpc<ColorsOfJoyPlugin, string> 
{
    public RpcSyncColors(ColorsOfJoyPlugin plugin, uint id) : base(plugin, id)
    {
    }
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
    public override void Write(MessageWriter writer, string data)
    {
        writer.Write(data);
    }

    public override string Read(MessageReader reader)
    {
        return reader.ReadString();
    }

    public override void Handle(PlayerControl innerNetObject, string data)
    {
        PluginSingleton<ColorsOfJoyPlugin>.Instance.Log.LogInfo("Received json data: " + data);
        innerNetObject.GetComponent<PlayerColorData>().SetColor(JsonSerializer.Deserialize<CustomPlayerColor>(data, new JsonSerializerOptions()
        {
            IncludeFields = true,
            Converters = { new Color32JsonConverter() },
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        }));
    }
}