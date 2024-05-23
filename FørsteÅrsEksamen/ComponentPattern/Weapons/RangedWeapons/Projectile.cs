﻿using FørsteÅrsEksamen.CommandPattern;
using FørsteÅrsEksamen.ComponentPattern.Enemies;
using FørsteÅrsEksamen.GameManagement;
using FørsteÅrsEksamen.Other;
using Microsoft.Xna.Framework;
using Npgsql.Replication.PgOutput.Messages;
using System;

namespace FørsteÅrsEksamen.ComponentPattern.Weapons.RangedWeapons
{
    // Erik
    public class Projectile : Component
    {
        private float speed;
        private Vector2 direction;
        private float range;
        private Vector2 startPos;
        private Vector2 lerpTo;
        private Vector2 targetPos;

        public Projectile(GameObject gameObject) : base(gameObject)
        {
            this.speed = 100;
            this.range = 200;
        }

        public override void Start()
        {
            SpriteRenderer sr = GameObject.GetComponent<SpriteRenderer>();
            sr.SetLayerDepth(LayerDepth.Player);

            sr.SetSprite(TextureNames.WoodArrow);
            
        }

        public void SetValues(float rotation)
        {
            
            GameObject.Transform.Rotation = rotation;
            startPos = GameObject.Transform.Position;

            lerpTo = BaseMath.Rotate(startPos + new Vector2(0, range), rotation);
            SetDirection();
        }

        public void SetDirection()
        {            
            targetPos = InputHandler.Instance.MouseOnUI;

            direction = Vector2.Normalize(targetPos - GameObject.Transform.Position);

            float angle = (float)Math.Atan2(direction.Y, direction.X);
            GameObject.Transform.Rotation = angle;
        }

        public override void Update(GameTime gameTime)
        {
            Move(gameTime);
        }

        private void Move(GameTime gameTime)
        {
            //if (targetPos == Vector2.Zero) return;

            //GameObject.Transform.Position = Vector2.Lerp(startPos, targetPos, GameWorld.DeltaTime * speed);

            //if (Math.Abs(GameObject.Transform.Position.X - targetPos.X) < 0.01f &&
            //    Math.Abs(GameObject.Transform.Position.Y - targetPos.Y) < 0.01f)
            //{
            //    GameWorld.Instance.Destroy(GameObject);

            //}

            float distance = speed * GameWorld.DeltaTime;
            Vector2 step = direction * distance;

            GameObject.Transform.Position += step;

            if (Vector2.Distance(startPos, GameObject.Transform.Position)>=range)
            {
                GameWorld.Instance.Destroy(GameObject);
            }
                        
        }

        private bool colCalled;

        public override void OnCollisionEnter(Collider collider)
        {
            //if (colCalled) return;
            if (collider.GameObject.GetComponent<Enemy>() != null)
            {
                GameWorld.Instance.Destroy(GameObject);
                //colCalled = true;
            }
        }

       

       

        public void Attack()
        {
        }
    }
}