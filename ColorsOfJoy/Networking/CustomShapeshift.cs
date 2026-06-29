using System.Text.Json;
using System.Text.Json.Serialization;
using ColorsOfJoy.Components;
using ColorsOfJoy.Converters;
using ColorsOfJoy.Patches;
using Reactor.Networking.Attributes;

namespace ColorsOfJoy.Networking;

public static class CustomShapeshift
{
    [MethodRpc((uint)CojRpcCalls.CustomShapeshift)]
    public static void RpcCustomShapeshift(this PlayerControl pc, PlayerControl targetPlayer, bool animate)
    {
        pc.CustomShapeshift(targetPlayer, animate);
    }
}