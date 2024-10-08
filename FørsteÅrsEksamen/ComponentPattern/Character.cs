﻿using ShamansDungeon.CommandPattern;
using ShamansDungeon.ComponentPattern.Particles.BirthModifiers;
using ShamansDungeon.ComponentPattern.Particles.Modifiers;
using ShamansDungeon.ComponentPattern.Particles.Origins;
using ShamansDungeon.ComponentPattern.Particles;
using ShamansDungeon.ComponentPattern.PlayerClasses;
using ShamansDungeon.ComponentPattern.Weapons;
using ShamansDungeon.ComponentPattern.WorldObjects;
using ShamansDungeon.GameManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using SharpDX.X3DAudio;
using ShamansDungeon.ComponentPattern.Path;
using ShamansDungeon.Other;
using System;
using ShamansDungeon.ComponentPattern.Enemies;
using ShamansDungeon.ComponentPattern.Weapons.MeleeWeapons;
using Microsoft.Xna.Framework.Audio;

namespace ShamansDungeon.ComponentPattern;

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
    public string Name { get; set; } = "";
    public static Vector2 NormalSpriteOffset { get; private set; } = new(0, -32); // Move the animation up a bit so it looks like it walks correctly.
    /// <summary>
    /// Can be changed if a custom sprite offset is needed. Otherwise it uses NormalSpriteOffset
    /// </summary>
    protected Vector2 SpriteOffset;
    
    public GameObject WeaponGo;
    public Vector2 Direction { get; protected set; } 
    public CharacterState State { get; protected set; } = CharacterState.Moving; // We use the method SetState, to we can change the animations and other variables.
    public int CollisionNr { get; set; }
    public bool CanAttack { get; set; } = true;

    protected bool IsEnemy;
    public SpriteRenderer SpriteRenderer { get; protected set; }
    protected Animator Animator;
    protected Collider Collider { get; set; }
    public Weapon Weapon { get; protected set; }
    protected Health Health;

    protected Dictionary<CharacterState, AnimNames> CharacterStateAnimations = new();
    protected AnimationDirectionState DirectionState = AnimationDirectionState.Right;

    private int normalSpeed;
    public int Speed 
    {
        get
        {
            return (int)(normalSpeed * SpeedMultiplier);
        }
        set
        {
            normalSpeed = value;
        }
    }

    public float SpeedMultiplier = 1;
    public float DamageMultiplier = 1;
    
    protected Grid Grid;

    protected SoundNames[] CharacterHitHurt { get; set; }
    protected int MaxAmountCharacterHitSoundsPlaying = 3;
    protected SoundNames[] CharacterDeath { get; set; }
    private SoundEffectInstance _characterHitHurt, _characterDeath;
    private float _maxHitHurtVolume = 1.0f;
    private float _maxDeathVolume = 1.0f;

    #region DamageEmitter
    public ParticleEmitter DamageTakenEmitter { get; private set; }
    protected Color[] DamageTakenAmountTextColor = new Color[] { Color.OrangeRed, Color.DarkRed, Color.Transparent };
    #endregion

    #region DustCloudEmitter
    private ParticleEmitter _dustCloudEmitter;
    private const float _startTimeToRotate = 0.3f;                // Initial time to complete rotation
    private const float _amountToRotationWhenWalking = 0.05f;     // Amount to rotate when walking
    private float _walkRotationTimer;                             // Timer for rotation
    private float _timeToRotate = _startTimeToRotate;              // Current time to complete rotation
    private bool _rotateRight = true;                             // Direction of rotation
    private bool _firstRot = true;                                // Flag for initial rotation
    private float _startRot;                                      // Starting rotation value

    #endregion

    #region Blood Emitter
    private Color[] _playerBlood = new Color[] { new Color(100, 40, 40, 255), new Color(50, 10, 10, 255) }; // 
    private Color[] _orcBlood = new Color[] { new Color(75, 114, 65, 255), new Color(35, 66, 42, 255) };
    private ParticleEmitter _bloodCloud { get; set; }
    private float _angle;
    private float _cone = MathHelper.PiOver4 * 0.5f;
    private float _bloodPrDmg = 4.0f;
    private int _maxBloodParticles = 200;
    #endregion

    #endregion Properties

    public Character(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Awake()
    {
        Grid = GridManager.Instance.CurrentGrid;

        SpriteRenderer = GameObject.GetComponent<SpriteRenderer>();
        Animator = GameObject.GetComponent<Animator>();
        Collider = GameObject.GetComponent<Collider>();

        Health = GameObject.GetComponent<Health>();
        SetActionInHealth();

        if (GameObject.GetComponent<Enemy>() != null)
        {
            IsEnemy = true;
        }
        else IsEnemy = false;

        if (WeaponGo != null)
        {
            Weapon = WeaponGo.GetComponent<Weapon>();
            Weapon.MoveWeaponAndAngle();
        }

        // If there isnt a custom offset
        if (SpriteOffset == Vector2.Zero)
        {
            SpriteOffset = NormalSpriteOffset;
        }
        //MakeEmptyHand();
        MakeEmitters();
    }
    private void MakeEmptyHand()
    {
        GameObject go = new();
        go.Transform.Position = GameObject.Transform.Position;

        EmptyHandSr = go.AddComponent<SpriteRenderer>();
        EmptyHandSr.SetLayerDepth(SpriteRenderer.LayerDepth, 0.001f);
        EmptyHandSr.SetSprite(TextureNames.HumanHandLeft);
        EmptyHandGo = go;
        GameWorld.Instance.Instantiate(go);
    }
    protected GameObject EmptyHandGo;
    protected SpriteRenderer EmptyHandSr;


    protected abstract float GetWeaponAngle();

    public virtual void MoveWeaponPosAndAngle()
    {
        Vector2 userPos = GameObject.Transform.Position;

        if (Weapon.Attacking)
        {
            // Lock the offset
            Weapon.SetPosition(userPos + Weapon.LastOffSetPos);
            return;
        }

        Weapon.WeaponAngleToUser = GetWeaponAngle();

        // Adjust the angle to be in the range of 0 to 2π
        if (Weapon.WeaponAngleToUser < 0)
        {
            Weapon.WeaponAngleToUser += 2 * MathHelper.Pi;
        }

        Weapon.LastOffSetPos = BaseMath.Rotate(Weapon.StartRelativePos, Weapon.WeaponAngleToUser - MathHelper.PiOver2) + Weapon.StartRelativeOffsetPos;
        Weapon.SetPosition(userPos + Weapon.LastOffSetPos);

        Weapon.SetAngleToCorrectSide(); // Also sets the private variables

        //if (IsEnemy && !CanAttack) return;
        ChangeWeaponAngleToUser();

        Weapon.StartAnimationAngle = Weapon.WeaponAngleToUser;
        Weapon.SetRotation(Weapon.StartAnimationAngle);
    }

    protected virtual void ChangeWeaponAngleToUser() { }
    private void SetActionInHealth()
    {
        Health.OnZeroHealth += OnDie;
        Health.OnDamageTaken += OnDamageTaken;
        //Health.OnResetColor += OnResetColor;
    }

    public override void Update()
    {
        base.Update();

        UpdateSoundEffects();
    }

    public Cell SetStartCollisionNr()
    {
        if (Grid == null) return null;

        GameObject currentCellGo = Grid.GetCellGameObjectFromPoint(GameObject.Transform.GridPosition);
        SetCharacterPos(currentCellGo.Transform.Position);
        Cell cell = currentCellGo.GetComponent<Cell>();
        CollisionNr = cell.CollisionNr;
        return cell;
    }
    protected Vector2 EmptyHandOffset = new Vector2(-25, 5);

    public void SetCharacterPos(Vector2 newPos)
    {
        GameObject.Transform.Position = newPos;
        if (EmptyHandGo == null) return;

        EmptyHandGo.Transform.Position = newPos + EmptyHandOffset;
    }

    public void SetSpriteEffects(SpriteEffects newSpriteEffect)
    {
        SpriteRenderer.SpriteEffects = newSpriteEffect;
        if (EmptyHandSr == null) return;
        EmptyHandSr.SpriteEffects = newSpriteEffect;
    }

    public void SetRotation(float newRot)
    {
        GameObject.Transform.Rotation = newRot;
        if (EmptyHandGo == null) return;
        EmptyHandGo.Transform.Rotation = newRot * 3;
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

        if (State == CharacterState.Moving)
            _dustCloudEmitter.PlayEmitter();
        else
            _dustCloudEmitter.StopEmitter();

        switch (State)
        {
            case CharacterState.Idle:
                Animator.PlayAnimation(CharacterStateAnimations[State]);
                SpriteRenderer.SetOriginOffset(new Vector2(16, 16));
                break;

            case CharacterState.Moving:
                Animator.PlayAnimation(CharacterStateAnimations[State]);
                SpriteRenderer.SetOriginOffset(new Vector2(32, 48));
                break;

            case CharacterState.Attacking:
                Animator.PlayAnimation(CharacterStateAnimations[CharacterState.Idle]); // Just uses the Idle since we have no attacking animation
                SpriteRenderer.SetOriginOffset(new Vector2(16, 16));
                break;

            case CharacterState.Dead:
                Animator.PlayAnimation(CharacterStateAnimations[State]);
                SpriteRenderer.SetOriginOffset(new Vector2(32, 48));
                Animator.StopCurrentAnimationAtLastSprite();
                break;
        }

        SpriteRenderer.DrawPosOffSet = SpriteOffset;
    }

    /// <summary>
    /// Updates the direction of which way the sprite should draw. Remember to set the direction!
    /// </summary>
    protected virtual void UpdateDirection()
    {
        if (Direction.X >= 0)
        {
            DirectionState = AnimationDirectionState.Right;
            SetSpriteEffects(SpriteEffects.None);
        }
        else if (Direction.X < 0)
        {
            DirectionState = AnimationDirectionState.Left;
            SetSpriteEffects(SpriteEffects.FlipHorizontally);
        }
    }

    public virtual void Attack()
    {
        if (Weapon == null) return;
        Weapon.MoveWeaponAndAngle(); // Should maybe wait till it has reached towards the new direction
        if (Weapon == null) return;
        Weapon.StartAttack();
    }

    private void OnDie()
    {
        SetState(CharacterState.Dead);

        if (CharacterDeath != null && CharacterDeath.Length > 0)
        {
            _characterDeath = GlobalSounds.PlayRandomizedSound(CharacterDeath, 2, 1.0f, true);
        }

        Weapon.RemoveGameObject();
        Weapon = null;
        // Remove hands
        if (EmptyHandGo != null)
        {
            GameWorld.Instance.Destroy(EmptyHandGo);
            EmptyHandSr = null;
        }

        // Remove hands
        SpriteRenderer.Color = Color.LightPink;

        Health.OnZeroHealth -= OnDie;
    }

    private void OnDamageTaken()
    {
        if (_characterHitHurt != null && _characterHitHurt.State == SoundState.Playing) return;

        // Play random sound from array
        _characterHitHurt = GlobalSounds.PlayRandomizedSound(CharacterHitHurt, MaxAmountCharacterHitSoundsPlaying, 1.0f, true);
    }
    
    private void UpdateSoundEffects()
    {
        GlobalSounds.ChangeSoundVolumeDistance(GameObject.Transform.Position, 150, 550, _maxHitHurtVolume, _characterHitHurt);
        GlobalSounds.ChangeSoundVolumeDistance(GameObject.Transform.Position, 150, 550, _maxDeathVolume, _characterDeath);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!InputHandler.Instance.DebugMode) return;
        // Draws the center of the sprite, with a 10x10 centered pixel
        Vector2 center = GameObject.Transform.Position - new Vector2(5, 5);
        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], center, null, Color.DarkRed, 0f, Vector2.Zero, 10, SpriteEffects.None, 1);
    }


    // Resets rotation when character is idle
    // Should maybe make a check to make sure that its not resetting too often.
    protected void ResetRotationWhenIdle()
    {
        if (GameObject.Transform.Rotation == 0f) return; // Aldready have reseted the rotation
        GameObject.Transform.Rotation = 0f; // Lerp back to rotation 0
        _firstRot = true;
        _walkRotationTimer = 0f;
        _startRot = 0f;
    }

    // Rotates character when moving
    protected void RotateCharacterOnMove(bool hasMoved)
    {
        if (!hasMoved)
        {
            ResetRotationWhenIdle();
            return;
        }

        // Adjust time to rotate based on initial or subsequent rotations
        if (_firstRot)
            _timeToRotate = _startTimeToRotate / 2;
        else
            _timeToRotate = _startTimeToRotate;

        if (_walkRotationTimer < _timeToRotate)
            _walkRotationTimer += (float)GameWorld.DeltaTime;

        // Normalize time for smooth rotation
        float normalized = _walkRotationTimer / _timeToRotate;
        normalized = BaseMath.EaseInOutQuad(normalized);

        float endRot;
        if (_rotateRight)
        {
            endRot = _amountToRotationWhenWalking;
            GameObject.Transform.Rotation = MathHelper.Lerp(_startRot, endRot, normalized);
            if (_timeToRotate - _walkRotationTimer < 0.05f)
            {
                // Reset the rot
                _rotateRight = false;
                _walkRotationTimer = 0f;
                _startRot = endRot;

                if (_firstRot) _firstRot = false;
            }
        }
        else
        {
            endRot = -_amountToRotationWhenWalking;

            GameObject.Transform.Rotation = MathHelper.Lerp(_startRot, endRot, normalized);
            if (_timeToRotate - _walkRotationTimer < 0.05f)
            {
                // Reset the rot
                _rotateRight = true;
                _startRot = endRot;
                _walkRotationTimer = 0f;
            }
        }

        SetRotation(GameObject.Transform.Rotation);
    }

    private void MakeEmitters()
    {
        MakeDustEmitter();
        MakeDamageTakenEmitter();
        
        MakeBlodEmitter();

        Health.AttackerPositionDamageTaken += SetBloodDirection;
        Health.AmountDamageTaken += SetBloodAmountOfParticles;
        Health.AmountDamageTaken += OnDamageTakenText;
    }

    private void SetBloodAmountOfParticles(int damage)
    {
        // If this is a enemy, we dont multiple since the other would be a player
        if (!IsEnemy) damage = (int)(damage * 3);

        int minAmount = Math.Min((int)(damage * _bloodPrDmg), _maxBloodParticles);
        _bloodCloud.MaxParticlesPerSecond = minAmount;
        _bloodCloud.ParticlesPerSecond = _bloodCloud.MaxParticlesPerSecond;
    }

    private void MakeBlodEmitter()
    {
        // Would need to make the direction a cone, so change that part. 
        GameObject go = EmitterFactory.CreateParticleEmitter("Blood Cloud", GameObject.Transform.Position, new Interval(300, 400), new Interval(0, MathHelper.PiOver2), _maxBloodParticles, new Interval(1000, 5000), 1000, 0.1f, new Interval(-MathHelper.Pi, 0), new Interval(-0.001, 0.001));

        _bloodCloud = go.GetComponent<ParticleEmitter>();
        _bloodCloud.FollowGameObject(GameObject, new Vector2(0, 0));
        _bloodCloud.LayerName = LayerDepth.DamageParticles;
        _bloodCloud.AddBirthModifier(new TextureBirthModifier(TextureNames.Pixel4x4));
        _bloodCloud.AbrubtStop = true;

        Color[] color;
        if (GameObject.GetComponent<Player>() != null)
            color = _playerBlood;
        else color = _orcBlood;
        
        _bloodCloud.AddModifier(new ColorRangeModifier(color));
        _bloodCloud.AddModifier(new GravityModifier(30));
        _bloodCloud.AddModifier(new DragModifier(0.2f, 0.5f, 55, 1f));
        _bloodCloud.AddModifier(new ScaleModifier(2.5f, 1.5f));

        _bloodCloud.Origin = new RectangleOrigin(40, 20);

        GameWorld.Instance.Instantiate(go);
    }

    private void SetBloodDirection(Vector2 attackingPos)
    {
        // From this character to the character that has attacked it.
        Vector2 direction = GameObject.Transform.Position - attackingPos;
        _angle = (float)Math.Atan2(direction.Y, direction.X);

        _bloodCloud.Direction = new Interval(_angle - _cone, _angle + _cone);

        _bloodCloud.PlayEmitter();
    }

    private void MakeDustEmitter()
    {
        GameObject go = EmitterFactory.CreateParticleEmitter("Dust Cloud", new Vector2(200, -200), new Interval(50, 100), new Interval(-MathHelper.Pi, 0), 20, new Interval(500, 1000), 1000, -1, new Interval(-MathHelper.Pi, 0), new Interval(-0.01, 0.01));

        _dustCloudEmitter = go.GetComponent<ParticleEmitter>();
        _dustCloudEmitter.FollowGameObject(GameObject, new Vector2(0, 25));
        _dustCloudEmitter.LayerName = LayerDepth.EnemyUnder;
        _dustCloudEmitter.AddBirthModifier(new TextureBirthModifier(TextureNames.Pixel4x4));
        _dustCloudEmitter.AddModifier(new ColorRangeModifier(new Color[] { new(142, 94, 52), new(82, 61, 42), Color.Transparent }));

        _dustCloudEmitter.AddBirthModifier(new ScaleBirthModifier(new Interval(1, 2)));
        _dustCloudEmitter.AddModifier(new GravityModifier());

        _dustCloudEmitter.Origin = new RectangleOrigin(80, 30);

        GameWorld.Instance.Instantiate(go);
    }

    private void OnDamageTakenText(int damage)
    {
        DamageTakenEmitter.LayerName = LayerDepth.DamageParticles;
        DamageTakenEmitter.SetParticleText(new TextOnSprite() { Text = damage.ToString() });
        DamageTakenEmitter.EmitParticles();
    }

    private void MakeDamageTakenEmitter()
    {
        GameObject go = EmitterFactory.TextDamageEmitter(DamageTakenAmountTextColor, GameObject, new Vector2(-20, -95), new RectangleOrigin(50, 5));
        DamageTakenEmitter = go.GetComponent<ParticleEmitter>();

        GameWorld.Instance.Instantiate(go);
    }
}