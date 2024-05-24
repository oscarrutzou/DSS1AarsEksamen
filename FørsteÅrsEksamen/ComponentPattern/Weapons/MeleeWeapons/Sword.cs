﻿using FørsteÅrsEksamen.CommandPattern;
using FørsteÅrsEksamen.LiteDB;
using FørsteÅrsEksamen.GameManagement;
using Microsoft.Xna.Framework;

namespace FørsteÅrsEksamen.ComponentPattern.Weapons.MeleeWeapons
{
    public class Sword : MeleeWeapon
    {
        public Sword(GameObject gameObject) : base(gameObject)
        {
        }

        public Sword(GameObject gameObject, bool enemyWeapon) : base(gameObject, enemyWeapon)
        {
            AttackSpeed = 1.7f;
            Damage = 50;
            LerpFromTo = MathHelper.Pi;
        }

        public override void Start()
        {
            AttackSoundNames = new SoundNames[]
            {
                SoundNames.SwipeSlow1,
            };
            spriteRenderer.SetSprite(TextureNames.WoodSword);
            SetStartColliders(new Vector2(7.5f, 38), 5, 5, 6, 4); // Gets set in each of the weapons insted of here.
        }

        protected override void PlayerStartAttack()
        {
            if (WeaponUser.Direction.X >= 0)
            {
                // Right
                TotalLerp = LerpFromTo;
            }
            else if (WeaponUser.Direction.X < 0)
            {
                // Left
                TotalLerp = -LerpFromTo;
            }
        }

        protected override void EnemyStartAttack()
        {
            if (WeaponUser.Direction.X >= 0)
            {
                // Right
                TotalLerp = LerpFromTo;
            }
            else if (WeaponUser.Direction.X < 0)
            {
                // Left
                TotalLerp = -LerpFromTo;
            }
            //// Maybe input the Character to the weapon.
            //Vector2 playerPos = SaveData.Player.GameObject.Transform.Position;

            //Vector2 relativeToPlayer = WeaponUser.GameObject.Transform.Position - playerPos;

            //if (relativeToPlayer.X < 0f)
            //{
            //    // Right
            //    TotalLerp = LerpFromTo;
            //    // Change position
            //}
            //else
            //{
            //    //Left
            //    TotalLerp = -LerpFromTo;
            //    // Change position

            //}
        }
    }
}