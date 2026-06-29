using System.Text.Json;
using System.Text.Json.Serialization;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using Reactor.Networking.Attributes;

namespace ColorsOfJoy.Networking;

public static class SetColorRpc
{
    [MethodRpc((uint)CojRpcCalls.SetColor)]
    public static void RpcSetColor(this PlayerControl pc, string colorJson)
    {
        var color = JsonSerializer.Deserialize<CustomPlayerColor>(colorJson, new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new Color32JsonConverter() },
            IncludeFields = true
        });
        if (color == null) return;
        pc.GetComponent<PlayerColorData>().SetColor(color);
        pc.Data.UpdateHostPanelImage();
    }
}

public enum CojRpcCalls
{
    SetColor = 1,
    SyncColors,
    CustomShapeshift
}