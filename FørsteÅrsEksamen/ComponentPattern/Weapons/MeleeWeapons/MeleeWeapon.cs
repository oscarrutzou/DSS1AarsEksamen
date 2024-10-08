﻿using ShamansDungeon.CommandPattern;
using ShamansDungeon.ComponentPattern.PlayerClasses;
using ShamansDungeon.ComponentPattern.WorldObjects;
using ShamansDungeon.GameManagement;
using ShamansDungeon.GameManagement.Scenes;
using ShamansDungeon.Other;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShamansDungeon.ComponentPattern.Weapons.MeleeWeapons;

public class HitDamagePackage
{
    public double TimeOfHit {  get; set; }
    public bool WasRotatingBack { get; set; }
    public HitDamagePackage(double timeOfHit, bool wasRotatingBack)
    {
        TimeOfHit = timeOfHit;
        WasRotatingBack = wasRotatingBack;
    }
}

// Erik
public abstract class MeleeWeapon : Weapon
{
    protected bool IsRotatingBack; 
    protected List<CollisionRectangle> WeaponColliders = new();
    protected double MinimumTimeBetweenHits = 0.3f;

    private Dictionary<GameObject, HitDamagePackage> _hitGameObjects { get; set; } = new(); 

    private float _rotateBackStartRotation;
    private double _timeCooldownBetweenHits { get; set; }
    private double _totalElapsedTime;
    public float NormalizedFullAttack { get; private set; } // The normalized value for how far the attack is to be dont
    private float _fullAttackTimer;
    protected bool CanDealDamage = true;

    protected MeleeWeapon(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Update()
    {
        base.Update();

        _totalElapsedTime += GameWorld.DeltaTime;
        ResetHittedGameObjects();

        if (Attacking)
        {
            AttackAnimation();

            CheckCollisionAndDmg();
        }

        UpdateCollisionBoxesPos(GameObject.Transform.Rotation);
    }

    private void AttackAnimation()
    {
        AttackedTotalElapsedTime += GameWorld.DeltaTime;

        // This should be changed to another animation method if its a stab attack
        if (Animations[CurrentAnim].SelectedAttackType == WeaponAnimAttackTypes.TwoWaySlash)
        {
            AttackAnimTwoSlash();
        }
        //else if (Animations[CurrentAnim].SelectedAttackType == WeaponAnimAttackTypes.OneWaySlash)
        //{
        //    AttackAnimOneSlash();
        //}
        else if (Animations[CurrentAnim].SelectedAttackType == WeaponAnimAttackTypes.Stab)
        {
            AttackAnimStab();
        }
    }


    private void AttackAnimTwoSlash()
    {
        _fullAttackTimer += (float)GameWorld.DeltaTime;
        NormalizedFullAttack = _fullAttackTimer / Animations[CurrentAnim].TotalTime; 

        // First rotate current angle to start angle of the anim, before attacking
        if (!IsRotatingBack && AttackedTotalElapsedTime >= TimeBeforeNewDirection)
        {
            PlayAttackSound();

            AttackedTotalElapsedTime = 0f; // Reset totalElapsedTime
            IsRotatingBack = true;

            SetStartAngleToNextAnim();
            // Need to also set the new start point
            _rotateBackStartRotation = GameObject.Transform.Rotation;


            // Makes the weapon flip when rotating back
            if (SpriteRenderer.SpriteEffects == SpriteEffects.FlipHorizontally)
                SetSpriteEffects(SpriteEffects.None);
            else if (SpriteRenderer.SpriteEffects == SpriteEffects.None)
                SetSpriteEffects(SpriteEffects.FlipHorizontally);
        }

        float normalizedAttackingTime = (float)AttackedTotalElapsedTime / (float)TimeBeforeNewDirection;
        float easedTime = Animations[CurrentAnim].AnimationMethod(normalizedAttackingTime); // maybe switch between them.
        float adjustedEasedTime = easedTime * (normalizedAttackingTime);
        float finalLerp = StartAnimationAngle;

        if (!IsRotatingBack)
        {
            finalLerp += FinalLerp; // The first rotation
            // Down attack
            SetRotation(MathHelper.Lerp(StartAnimationAngle, finalLerp, adjustedEasedTime));

        }
        else
        {
            // Second rotation to rotate to the start of the next rotation
            //Up attack
            SetRotation(MathHelper.Lerp(_rotateBackStartRotation, StartAnimationAngle, adjustedEasedTime));
        }

        // Reset
        if (NormalizedFullAttack >= 1f)
        {
            IsRotatingBack = false;
            Attacking = false;
            FinnishedAttack = true;
            ResetAttackTimers();
        }
    }


    private void ResetAttackTimers()
    {
        _fullAttackTimer = 0f;
        NormalizedFullAttack = 0f;
        AttackedTotalElapsedTime = 0f;
    }

    public override void SetAttackDirection()
    {
        ResetAttackTimers();

        _timeCooldownBetweenHits = Math.Max(MinimumTimeBetweenHits, TimeBeforeNewDirection / 3);

        if (LeftSide)
            FinalLerp = -Animations[CurrentAnim].AmountOfRotation;
        else
            FinalLerp = Animations[CurrentAnim].AmountOfRotation;
    }

    private void AttackAnimStab()
    {
        /*
         * How should the stab work.
         */
    }

    public void CheckCollisionAndDmg()
    {
        // Could use a mesh, that just contains the different types that it can hit, and go though each of those lists

        if (!CanDealDamage) return;

        CheckCollisionCharacters();
        CheckCollisionBreakableItems();
    }

    private void CheckCollisionCharacters()
    {
        GameObjectTypes type;
        if (EnemyUser != null)
            type = GameObjectTypes.Player;
        else
            type = GameObjectTypes.Enemy;

        foreach (GameObject otherGo in SceneData.Instance.GameObjectLists[type])
        {
            if (!otherGo.IsEnabled || _hitGameObjects.ContainsKey(otherGo)) continue;

            Collider otherCollider = otherGo.GetComponent<Collider>();

            if (otherCollider == null) continue;
            foreach (CollisionRectangle weaponRectangle in WeaponColliders)
            {
                if (_hitGameObjects.ContainsKey(otherGo)) break; // Need to check again here so it dosent attack twice

                if (weaponRectangle.Rectangle.Intersects(otherCollider.CollisionBox))
                {
                    _hitGameObjects.Add(otherGo, new(_totalElapsedTime, IsRotatingBack));
                    DealDamage(otherGo);
                    break;
                }
            }
        }
    }

    private void CheckCollisionBreakableItems()
    {
        if (EnemyUser != null) return; // Only player can destroy items

        foreach (GameObject otherGo in SceneData.Instance.GameObjectLists[GameObjectTypes.BreakableItems])
        {
            if (!otherGo.IsEnabled || _hitGameObjects.ContainsKey(otherGo)) continue;

            Collider otherCollider = otherGo.GetComponent<Collider>();

            if (otherCollider == null) continue;
            foreach (CollisionRectangle weaponRectangle in WeaponColliders)
            {
                if (_hitGameObjects.ContainsKey(otherGo)) break; // Need to check again here so it dosent attack twice

                if (weaponRectangle.Rectangle.Intersects(otherCollider.CollisionBox))
                {
                    _hitGameObjects.Add(otherGo, new(_totalElapsedTime, IsRotatingBack));
                    DealDamage(otherGo);
                    break;
                }
            }
        }
    }

    private void ResetHittedGameObjects()
    {
        Dictionary<GameObject, HitDamagePackage> temp = _hitGameObjects;
        foreach (GameObject go in temp.Keys)
        {
            HitDamagePackage package = temp[go];
            double difference = _totalElapsedTime - package.TimeOfHit;
            // Need also to check that its not the same IsRotatingBack
            if (difference >= _timeCooldownBetweenHits && IsRotatingBack != package.WasRotatingBack)
            {
                _hitGameObjects.Remove(go);
            }
        }

    }

    public void DealDamage(GameObject damageGo)
    {
        Health health = damageGo.GetComponent<Health>();

        if (!health.IsDead)
        {
            PlayHitSound();
        }

        // Float so we can divide with enemy weakness
        float damage = Animations[CurrentAnim].Damage * User.DamageMultiplier;
        if (EnemyUser != null)
            damage *= EnemyWeakness;

        health.TakeDamage((int)damage, User.GameObject.Transform.Position);
    }


    #region Weapon Colliders

    private void UpdateCollisionBoxesPos(float rotation)
    {
        foreach (CollisionRectangle collisionRectangle in WeaponColliders)
        {
            // Calculate the position relative to the center of the weapon
            Vector2 relativePos = collisionRectangle.StartRelativePos;

            // Rotate the relative position
            Vector2 newPos = BaseMath.Rotate(relativePos, rotation);

            // Set the collision rectangle position based on the rotated relative position
            collisionRectangle.Rectangle.X = (int)(GameObject.Transform.Position.X + newPos.X) - collisionRectangle.Rectangle.Width / 2;
            collisionRectangle.Rectangle.Y = (int)(GameObject.Transform.Position.Y + newPos.Y) - collisionRectangle.Rectangle.Height / 2;
        }
    }

    /// <summary>
    /// To make colliders for the weapon.
    /// </summary>
    /// <param name="origin">How far the origin is from the top left corner. Should have a -0.5f in X to make it centered.</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="heightFromOriginToHandle">Height from the origin on the sprite to the end of the handle</param>
    /// <param name="amountOfColliders"></param>
    protected void SetStartColliders(Vector2 origin, int width, int height, int heightFromOriginToHandle, int amountOfColliders)
    {
        WeaponColliders.Clear();
        SpriteRenderer.SetOriginOffset(origin);
        AddWeaponColliders(width, height, heightFromOriginToHandle, amountOfColliders);
    }

    /// <summary>
    /// The colliders on our weapon, used for collision between characters
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="heightFromOriginToHandle">Height from the origin on the sprite to the end of the handle</param>
    /// <param name="amountOfColliders"></param>
    private void AddWeaponColliders(int width, int height, int heightFromOriginToHandle, int amountOfColliders)
    {
        Vector2 pos = GameObject.Transform.Position;
        Vector2 scale = GameObject.Transform.Scale;

        pos += new Vector2(0, -heightFromOriginToHandle * scale.Y); // Adds the height from origin to handle

        // Adds the weapon colliders
        for (int i = 0; i < amountOfColliders; i++)
        {
            pos += new Vector2(0, -height * scale.Y);

            WeaponColliders.Add(new CollisionRectangle()
            {
                Rectangle = MakeRec(pos, width, height, scale),
                StartRelativePos = pos
            });
        }
    }

    private Rectangle MakeRec(Vector2 pos, int width, int height, Vector2 scale) => new Rectangle((int)pos.X, (int)pos.Y, width * (int)scale.X, (int)scale.Y * height);

    #endregion Weapon Colliders

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!InputHandler.Instance.DebugMode) return;

        foreach (CollisionRectangle collisionRectangle in WeaponColliders)
        {
            Collider.DrawRectangleNoSprite(collisionRectangle.Rectangle, Color.OrangeRed, spriteBatch);
        }
        base.Draw(spriteBatch);
    }
}