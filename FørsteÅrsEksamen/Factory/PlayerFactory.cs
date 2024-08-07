﻿using DoctorsDungeon.ComponentPattern;
using DoctorsDungeon.ComponentPattern.PlayerClasses;
using DoctorsDungeon.ComponentPattern.Weapons;
using DoctorsDungeon.ComponentPattern.WorldObjects;
using DoctorsDungeon.LiteDB;
using Microsoft.Xna.Framework;

namespace DoctorsDungeon.Factory;

public enum ClassTypes
{
    Rogue,
    Warrior,
    Mage,
}

// Stefan
public static class PlayerFactory
{
    public static GameObject Create(ClassTypes playerClass, WeaponTypes weaponType)
    {
        GameObject playerGo = new GameObject();

        playerGo.Type = GameObjectTypes.Player;

        playerGo.AddComponent<SpriteRenderer>();
        playerGo.AddComponent<Animator>();
        playerGo.AddComponent<Collider>();

        GameObject hands = CreateHands();
        GameWorld.Instance.Instantiate(hands); // Makes hands

        GameObject movementColliderGo = CreatePlayerMovementCollider();
        GameWorld.Instance.Instantiate(movementColliderGo); // Makes the collider

        // remove the hands from the constructer
        playerGo = AddClassComponent(playerGo, playerClass);

        Player player = playerGo.GetComponent<Player>();
        // Adds hands and the collider
        player.HandsGo = hands;
        player.MovementColliderGo = movementColliderGo;

        Health health = playerGo.AddComponent<Health>();

        //Weapon
        GameObject weaponGo = WeaponFactory.Create(weaponType);
        weaponGo.GetComponent<Weapon>().PlayerUser = player;
        GameWorld.Instance.Instantiate(weaponGo);

        // Add weapon to player
        player.WeaponGo = weaponGo;
        player.WeaponType = weaponType;
        player.ClassType = playerClass;

        if (GameWorld.DebugAndCheats)
        {
            health.SetHealth(999_999);
        }

        // Set the reference to this player.
        SaveData.Player = player;

        return playerGo;
    }

    private static GameObject AddClassComponent(GameObject playerGo, ClassTypes playerClass)
    {
        switch (playerClass)
        {
            case ClassTypes.Warrior:
                playerGo.AddComponent<Warrior>();
                break;

            case ClassTypes.Rogue:
                playerGo.AddComponent<Archer>();
                break;

            case ClassTypes.Mage:
                playerGo.AddComponent<Mage>();
                break;
        }

        return playerGo;
    }

    private static GameObject CreateHands()
    {
        GameObject go = new();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Animator>();
        return go;
    }

    private static GameObject CreatePlayerMovementCollider()
    {
        GameObject go = new();
        go.AddComponent<SpriteRenderer>().SetLayerDepth(LayerDepth.Player);
        Collider collider = go.AddComponent<Collider>();

        collider.SetCollisionBox(14, 15, new Vector2(0, -5));
        collider.DebugColor = Color.Aqua;

        return go;
    }
}