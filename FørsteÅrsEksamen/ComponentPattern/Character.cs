﻿using DoctorsDungeon.CommandPattern;
using DoctorsDungeon.ComponentPattern.Weapons;
using DoctorsDungeon.GameManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DoctorsDungeon.ComponentPattern;

public enum AnimationDirectionState
{
    Left,
    Right
}

public enum CharacterState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

// Oscar
public abstract class Character : Component
{
    #region Properties

    public GameObject WeaponGo, HandLeft, HandRight;

    protected SpriteRenderer SpriteRenderer;
    protected Animator Animator;
    protected Collider Collider;
    protected Weapon Weapon;

    protected Dictionary<CharacterState, AnimNames> CharacterStateAnimations = new();
    protected Vector2 SmallSpriteOffset = new(0, -32); // Move the animation up a bit so it looks like it walks correctly.
    protected Vector2 LargeSpriteOffSet = new(0, -96); // Move the animation up more since its a 64x64 insted of 32x32 canvans, for the Run and Death.

    public CharacterState State { get; protected set; } = CharacterState.Moving; // We use the method SetState, to we can change the animations and other variables.
    public Vector2 Direction { get; protected set; }
    protected AnimationDirectionState DirectionState = AnimationDirectionState.Right;

    protected float AttackTimer;
    protected float AttackCooldown = 2f;

    protected int Speed { get; set; }
    public int CurrentHealth = 100;
    public int MaxHealth = 100;
    public int RoomNr { get; set; }

    #endregion Properties

    public Character(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Awake()
    {
        SpriteRenderer = GameObject.GetComponent<SpriteRenderer>();
        Animator = GameObject.GetComponent<Animator>();
        Collider = GameObject.GetComponent<Collider>();

        if (WeaponGo != null)
        {
            Weapon = WeaponGo.GetComponent<Weapon>();
            Weapon.MoveWeapon();
        }
    }

    // This is not a abstract method since we only need to set it in the Player and Enemy class, and not in its subclasses
    /// <summary>
    /// A method to set the new state and change the animation drawn.
    /// </summary>
    /// <param name="newState"></param>
    protected virtual void SetState(CharacterState newState)
    {
        if (State == newState) return; // Dont change the state to the same and reset the animation
        State = newState;

        switch (State)
        {
            case CharacterState.Idle:
                Animator.PlayAnimation(CharacterStateAnimations[State]);

                SpriteRenderer.OriginOffSet = SmallSpriteOffset;
                break;

            case CharacterState.Moving:
                Animator.PlayAnimation(CharacterStateAnimations[State]);

                SpriteRenderer.OriginOffSet = LargeSpriteOffSet;
                break;

            case CharacterState.Attacking:
                Animator.PlayAnimation(CharacterStateAnimations[CharacterState.Idle]); // Just uses the Idle since we have no attacking animation

                SpriteRenderer.OriginOffSet = SmallSpriteOffset;
                break;

            case CharacterState.Dead:
                Animator.PlayAnimation(CharacterStateAnimations[State]);

                SpriteRenderer.OriginOffSet = LargeSpriteOffSet;
                Animator.StopCurrentAnimationAtLastSprite();
                break;
        }
    }

    /// <summary>
    /// Updates the direction of which way the sprite should draw. Remember to set the direction!
    /// </summary>
    protected virtual void UpdateDirection()
    {
        if (Direction.X >= 0)
        {
            DirectionState = AnimationDirectionState.Right;
            SpriteRenderer.SpriteEffects = SpriteEffects.None;
        }
        else if (Direction.X < 0)
        {
            DirectionState = AnimationDirectionState.Left;
            SpriteRenderer.SpriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public void Attack()
    {
        if (Weapon == null) return;
        Weapon.MoveWeapon(); // Should maybe wait till it has reached towards the new direction
        if (Weapon == null) return; 
        Weapon.StartAttack();
    }

    public void TakeDamage(int damage)
    {
        int newHealth = CurrentHealth - damage;

        if (newHealth < 0) CurrentHealth = 0;
        else CurrentHealth = newHealth;

        if (CurrentHealth > 0) return;

        Die();
    }

    public virtual void Die()
    {
        SetState(CharacterState.Dead);
        GameWorld.Instance.Destroy(WeaponGo);
        Weapon = null;

        // Remove hands
        SpriteRenderer.Color = Color.LightPink;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!InputHandler.Instance.DebugMode) return;
        Vector2 center = GameObject.Transform.Position - new Vector2(5, 5);
        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], center, null, Color.DarkRed, 0f, Vector2.Zero, 10, SpriteEffects.None, 1);
    }
}