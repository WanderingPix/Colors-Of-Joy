using System;
using JetBrains.Annotations;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace ColorsOfJoy.Components;

[RegisterInIl2Cpp]
public class ExtendedPoolablePlayer(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PoolablePlayer player;
    public PlayerControl owner;
    #nullable enable
    public ChatBubble? bubble;
    public PlayerVoteArea? voteArea;
    private void Start()
    {
        player = GetComponent<PoolablePlayer>();
    }

    private void FixedUpdate()
    {
        if (player == null || owner == null) return;
        var col = owner.GetComponent<PlayerColorData>().Color;
        
        foreach (var rend in GetComponentsInChildren<SpriteRenderer>())
        {
            PlayerMaterialExtensions.SetColors(col, rend.material);
        }

        player.cosmetics.colorBlindText.text = col.Name;
        
        bubble?.ColorBlindName.text = col.Name;
        voteArea?.ColorBlindName.text = col.Name;
    }
}