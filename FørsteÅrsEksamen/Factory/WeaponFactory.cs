﻿using FørsteÅrsEksamen.ComponentPattern;
using FørsteÅrsEksamen.ComponentPattern.Weapons.MeleeWeapons;
using FørsteÅrsEksamen.ComponentPattern.Weapons.RangedWeapons;
using Microsoft.Xna.Framework;

namespace FørsteÅrsEksamen.Factory
{
    public enum WeaponTypes
    {
        Sword,
        Axe,
        MagicStaff,
        MagicStaffFire,
        Bow,
        BowFire,
    }

    internal static class WeaponFactory
    {
        public static GameObject Create(WeaponTypes type)
        {
            GameObject weaponGo = new GameObject();
            weaponGo.Transform.Scale = new Vector2(4, 4);
            weaponGo.AddComponent<SpriteRenderer>();
            AddClassComponent(weaponGo, type);

            return weaponGo;
        }

        private static GameObject AddClassComponent(GameObject weaponGo, WeaponTypes type)
        {
            switch (type)
            {
                case WeaponTypes.Sword:
                    weaponGo.AddComponent<Sword>();
                    break;

                case WeaponTypes.Axe:
                    weaponGo.AddComponent<Axe>();
                    break;

                case WeaponTypes.MagicStaff:
                    weaponGo.AddComponent<MagicStaff>();
                    break;

                case WeaponTypes.MagicStaffFire:
                    weaponGo.AddComponent<MagicStaff>();
                    break;

                case WeaponTypes.Bow:
                    weaponGo.AddComponent<Bow>();
                    break;

                case WeaponTypes.BowFire:
                    weaponGo.AddComponent<MagicStaff>();
                    break;
            }

            return weaponGo;
        }
    }
}