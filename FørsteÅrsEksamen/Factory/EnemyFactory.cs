﻿using DoctorsDungeon.ComponentPattern;
using DoctorsDungeon.ComponentPattern.Enemies;
using DoctorsDungeon.ComponentPattern.Enemies.MeleeEnemies;
using DoctorsDungeon.ComponentPattern.Enemies.RangedEnemies;
using DoctorsDungeon.ComponentPattern.Weapons;
using DoctorsDungeon.ComponentPattern.WorldObjects;
using Microsoft.Xna.Framework;
using System;

namespace DoctorsDungeon.Factory;

public enum EnemyTypes
{
    OrcWarrior,
    OrcArcher,
    OrcShaman,
    SkeletonWarrior,
    SkeletonArcher,
    SkeletonMage,
}

// Asser
public static class EnemyFactory
{
    private static Random random = new();

    //public static GameObject CreateWithRandomType()
    //{
    //    Array enemyValue = Enum.GetValues(typeof(EnemyTypes));
    //    int randomClassIndex = random.Next(enemyValue.Length);
    //    EnemyTypes randomType = (EnemyTypes)enemyValue.GetValue(randomClassIndex);

    //    Array weaponValue = Enum.GetValues(typeof(WeaponTypes));
    //    int randomWeaponIndex = random.Next(weaponValue.Length);
    //    WeaponTypes randomWeapon = (WeaponTypes)weaponValue.GetValue(randomWeaponIndex);

    //    // Need to put them into classes

    //    return Create(randomType, randomWeapon);
    //}

    public static GameObject Create(EnemyTypes enemyType, WeaponTypes weaponType)
    {
        GameObject enemyGo = new GameObject();
        enemyGo.Type = GameObjectTypes.Enemy;
        enemyGo.AddComponent<SpriteRenderer>();
        enemyGo.AddComponent<Animator>();
        enemyGo.AddComponent<Collider>();
        enemyGo.AddComponent<Health>();

        enemyGo = AddEnemyComponent(enemyGo, enemyType);

        // Add weapon
        Enemy enemy = enemyGo.GetComponent<Enemy>();

        GameObject weaponGo = WeaponFactory.Create(weaponType);
        Weapon weapon = weaponGo.GetComponent<Weapon>();
        weapon.EnemyUser = enemy;
        GameWorld.Instance.Instantiate(weaponGo);

        enemy.WeaponGo = weaponGo;

        // Also add hands

        return enemyGo;
    }

    private static GameObject AddEnemyComponent(GameObject enemyGo, EnemyTypes enemytype)
    {
        switch (enemytype)
        {
            case EnemyTypes.OrcWarrior:
                enemyGo.AddComponent<OrcWarrior>();
                break;

            case EnemyTypes.OrcArcher:
                enemyGo.AddComponent<OrcArcher>();
                break;

            case EnemyTypes.SkeletonWarrior:
                enemyGo.AddComponent<SkeletonWarrior>();
                break;

            case EnemyTypes.SkeletonArcher:
                enemyGo.AddComponent<SkeletonArcher>();
                break;
        }

        return enemyGo;
    }

    private static GameObject CreateHands()
    {
        GameObject go = new();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Animator>();
        return go;
    }
}