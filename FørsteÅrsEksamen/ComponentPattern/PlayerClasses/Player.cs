﻿using DoctorsDungeon.ComponentPattern.Path;
using DoctorsDungeon.ComponentPattern.Weapons;
using DoctorsDungeon.ComponentPattern.WorldObjects;
using DoctorsDungeon.Factory;
using DoctorsDungeon.LiteDB;
using DoctorsDungeon.ObserverPattern;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DoctorsDungeon.ComponentPattern.PlayerClasses;

// Stefan
public abstract class Player : Character, ISubject
{
    public GameObject HandsGo;
    public GameObject MovementColliderGo;

    private Vector2 totalMovementInput, velocity, targetVelocity, previousPosition;

    private readonly float turnSpeed = 40f; // Adjust as needed

    protected List<IObserver> observers = new();

    public Potion ItemInInventory { get; set; }

    private Collider movementCollider;

    public WeaponTypes WeaponType; 
    public ClassTypes ClassType;

    private double onDeadTimer;
    private double timeTillSceneChange = 3f;

    public Player(GameObject gameObject) : base(gameObject)
    {
        Speed = 150;
    }

    public Player(GameObject gameObject, GameObject handsGo, GameObject movementColliderGo) : base(gameObject)
    {
        Speed = 150;
        this.HandsGo = handsGo;
        this.MovementColliderGo = movementColliderGo;
    }

    public override void Awake()
    {
        base.Awake();
        movementCollider = MovementColliderGo.GetComponent<Collider>();
        Collider.SetCollisionBox(15, 24, new Vector2(0, 30));
    }

    public override void Start()
    {
        SpriteRenderer.SetLayerDepth(LayerDepth.Player);
        SetState(CharacterState.Idle);
    }

    public override void Update()
    {
        if (State != CharacterState.Dead)
        {
            CheckForMovement();
        }

        Weapon?.MoveWeaponAndAngle();

        switch (State)
        {
            case CharacterState.Moving:
                Move(totalMovementInput);
                break;

            case CharacterState.Dead:
                ChangeScene();
                break;
        }

        totalMovementInput = Vector2.Zero;
    }

    private void ChangeScene()
    {
        onDeadTimer += GameWorld.DeltaTime;
        if (onDeadTimer >= timeTillSceneChange)
        {
            onDeadTimer = 0;
            SaveData.HasWon = false;
            DB.Instance.OnChangeSceneEnd();
        }
    }

    private void CheckForMovement()
    {
        //SpriteRenderer.Rotation += (float)GameWorld.DeltaTime;

        if (totalMovementInput != Vector2.Zero)
        {
            SetState(CharacterState.Moving);

        }
        else
        {
            SetState(CharacterState.Idle);
        }
    }

    #region Movement

    public void AddInput(Vector2 input) // 0, 1 / 0, -1 / 1,0 / -1, 0
    {
        // Ensure the input vector is not a zero vector, will cause Nan/Nan in the vector
        if (input != Vector2.Zero)
        {
            // Add the normalized input to the total movement
            totalMovementInput += input;

            totalMovementInput.Normalize();
        }
    }

    public void Move(Vector2 input)
    {
        InitializeMovement();

        if (input != Vector2.Zero)
        {
            ProcessInput(input);
        }

        UpdateDirection();

        if (GridManager.Instance.CurrentGrid == null) return; // Player can't walk if there is no grid.

        TryMoveInBothDirections();

        UpdatePositionAndNotify();
    }

    private void InitializeMovement()
    {
        targetVelocity = Vector2.Zero;
        previousPosition = GameObject.Transform.Position;
    }

    private void ProcessInput(Vector2 input) // 0.77 / 0.77
    {
        input.Normalize();
        targetVelocity = input * Speed * (float)GameWorld.DeltaTime;

        // To fix the error that if all buttons have been pressed, that it sometimes sets the velocity to Nan/Nan
        if (float.IsNaN(velocity.X))
        {
            velocity = Vector2.Zero;
        }

        // Interpolate the velocity
        velocity = Vector2.Lerp(velocity, targetVelocity, turnSpeed * (float)GameWorld.DeltaTime);
        Direction = velocity;
    }

    private void TryMoveInBothDirections()
    {
        // Separate the movement into X and Y components
        Vector2 xMovement = new Vector2(velocity.X, 0) * Speed * (float)GameWorld.DeltaTime;
        Vector2 yMovement = new Vector2(0, velocity.Y) * Speed * (float)GameWorld.DeltaTime;

        //bool hasMoved = false;
        // Try moving along the X axis
        if (TryMove(xMovement))
        {
            // Update the previous position after a successful move
            previousPosition = GameObject.Transform.Position;
            //hasMoved = true;
        }

        // Try moving along the Y axis
        if (TryMove(yMovement))
        {
            // Update the previous position after a successful move
            previousPosition = GameObject.Transform.Position;
            //hasMoved = true;
        }

    }

    private void UpdatePositionAndNotify()
    {
        SetMovement(GameObject.Transform.Position); // So we set the other gameobjects (Hands, Movement Collider...)

        // Updates the grid position
        GameObject cellGoUnderPlayer = GridManager.Instance.GetCellAtPos(GameObject.Transform.Position);
        GameObject.Transform.GridPosition = cellGoUnderPlayer.Transform.GridPosition;
        Cell cellUnderPlayer = cellGoUnderPlayer.GetComponent<Cell>();
        CollisionNr = cellUnderPlayer.CollisionNr;
        GridManager.Instance.AddVisitedRoomNumbers(cellUnderPlayer.RoomNr);

        Notify();
    }

    private bool TryMove(Vector2 movement)
    {
        // Translate the GameObject, and movement collider.
        TranslateMovement(movement);

        // Get the CollisionBox
        Rectangle collisionBox = movementCollider.CollisionBox;

        // Check each corner of the CollisionBox
        Vector2[] corners = new Vector2[]
        {
            new Vector2(collisionBox.Left, collisionBox.Top),
            new Vector2(collisionBox.Right, collisionBox.Top),
            new Vector2(collisionBox.Right, collisionBox.Bottom),
            new Vector2(collisionBox.Left, collisionBox.Bottom)
        };

        foreach (Vector2 corner in corners)
        {
            GameObject gridCell = GridManager.Instance.GetCellAtPos(corner);

            if (gridCell == null || gridCell.GetComponent<Cell>().CellWalkableType == CellWalkableType.NotValid)
            {
                // If any corner is in an invalid cell, revert the movement on both player and
                SetMovement(previousPosition);
                return false;
            }
        }

        // If all corners are in valid cells, the movement is successful
        return true;
    }

    /// <summary>
    /// Adds the movement to the gameobject
    /// </summary>
    /// <param name="movement"></param>
    protected void TranslateMovement(Vector2 movement)
    {
        GameObject.Transform.Translate(movement);
        MovementColliderGo.Transform.Position = GameObject.Transform.Position;
        HandsGo.Transform.Position = GameObject.Transform.Position;
        Weapon.MoveWeaponAndAngle();
    }

    /// <summary>
    /// Sets the position of the object to the position.
    /// </summary>
    /// <param name="position"></param>
    protected void SetMovement(Vector2 position)
    {
        GameObject.Transform.Position = position;
        MovementColliderGo.Transform.Position = GameObject.Transform.Position;
        HandsGo.Transform.Position = GameObject.Transform.Position;
        Weapon.MoveWeaponAndAngle();
        GameWorld.Instance.WorldCam.Position = GameObject.Transform.Position; //Sets the new position of the world cam
    }

    #endregion Movement

    #region Item

    public bool CanPickUpItem(Potion item)
    {
        if (ItemInInventory == null)
        {
            ItemInInventory = item;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UseItem()
    {
        if (ItemInInventory == null) return;

        ItemInInventory.Use();
    }

    #endregion Item

    #region Observer Pattern

    public void Attach(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (IObserver observer in observers)
        {
            observer.UpdateObserver();
        }
    }

    #endregion Observer Pattern
}