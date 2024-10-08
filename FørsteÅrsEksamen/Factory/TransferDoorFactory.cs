﻿using ShamansDungeon.ComponentPattern;
using ShamansDungeon.ComponentPattern.WorldObjects;
using ShamansDungeon.GameManagement;

namespace ShamansDungeon.Factory;

public static class TransferDoorFactory
{
    // Oscar
    public static GameObject Create()
    {
        GameObject go = new();

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.SetLayerDepth(LayerDepth.BackgroundDecoration);
        sr.SetSprite(TextureNames.DoorClosed);
        sr.IsCentered = false;

        Collider collider = go.AddComponent<Collider>();
        collider.CenterCollisionBox = false;

        go.AddComponent<TransferDoor>();

        return go;
    }
}