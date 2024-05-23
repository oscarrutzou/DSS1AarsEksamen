﻿using FørsteÅrsEksamen.ComponentPattern.Path;
using FørsteÅrsEksamen.ComponentPattern.Weapons;
using FørsteÅrsEksamen.Factory;
using FørsteÅrsEksamen.GameManagement;
using FørsteÅrsEksamen.ObserverPattern;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FørsteÅrsEksamen.ComponentPattern.Classes
{
    // Oscar
    public abstract class Player : Character, ISubject
    {
        public GameObject HandsGo;
        public GameObject MovementColliderGo;

        private Vector2 totalMovementInput, velocity, targetVelocity, previousPosition;

        private readonly float turnSpeed = 40f; // Adjust as needed

        protected List<IObserver> observers = new();

        public PickupableItem ItemInInventory;

        private Collider movementCollider;

        public WeaponTypes WeaponType = WeaponTypes.Sword;
        public ClassTypes ClassType = ClassTypes.Warrior;

        public Player(GameObject gameObject) : base(gameObject)
        {
            speed = 150;
        }

        public Player(GameObject gameObject, GameObject handsGo, GameObject movementColliderGo) : base(gameObject)
        {
            speed = 150;
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
            SpriteRenderer.SetLayerDepth(LAYERDEPTH.Player);
            SetState(CharacterState.Idle);
        }

        #region Movement
        public void AddInput(Vector2 input)
        {
            if (input != Vector2.Zero)
            {
                // Ensure the input vector is not a zero vector
                input.Normalize();

                // Add the normalized input to the total movement
                totalMovementInput += input;
            }
        }

        public void Move(Vector2 input)
        {
            targetVelocity = Vector2.Zero;

            previousPosition = GameObject.Transform.Position;

            if (input != Vector2.Zero)
            {
                input.Normalize();

                targetVelocity = input * speed * GameWorld.DeltaTime;

                // To fix the error that if all buttons have been pressed, that it sometimes sets the velocity to Nan/Nan
                if (float.IsNaN(velocity.X)) 
                {
                    velocity = Vector2.Zero;
                }

                // Interpolate the velocity
                velocity = Vector2.Lerp(velocity, targetVelocity, turnSpeed * GameWorld.DeltaTime);
                Direction = velocity;
            }

            UpdateDirection();

            if (GridManager.Instance.CurrentGrid == null) return; // Player cant walk if there is no grid.

            // Separate the movement into X and Y components
            Vector2 xMovement = new Vector2(velocity.X, 0) * speed * GameWorld.DeltaTime;
            Vector2 yMovement = new Vector2(0, velocity.Y) * speed * GameWorld.DeltaTime;

            bool hasMoved = false;
            // Try moving along the X axis
            if (TryMove(xMovement))
            {
                // Update the previous position after a successful move
                previousPosition = GameObject.Transform.Position;
                hasMoved = true;
            }

            // Try moving along the Y axis
            if (TryMove(yMovement))
            {
                // Update the previous position after a successful move
                previousPosition = GameObject.Transform.Position;
                hasMoved = true;
            }

            if (!hasMoved) return; // Dont need to set new position, since its the same.

            SetMovement(GameObject.Transform.Position); // So we set the other gameobjects (Hands, Movement Collider...)

            // Updates the grid position
            GameObject cellGoUnderPlayer = GridManager.Instance.GetCellAtPos(GameObject.Transform.Position);
            GameObject.Transform.GridPosition = cellGoUnderPlayer.Transform.GridPosition;

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
            Weapon.MoveWeapon(GameObject.Transform.Position);
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
            Weapon.MoveWeapon(GameObject.Transform.Position);
            GameWorld.Instance.WorldCam.position = GameObject.Transform.Position; //Sets the new position of the world cam
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (totalMovementInput != Vector2.Zero)
            {
                totalMovementInput = Vector2.Normalize(totalMovementInput);
                SetState(CharacterState.Moving);
            }
            else
            {
                SetState(CharacterState.Idle);
            }

            switch (State)
            {
                case CharacterState.Idle:
                    break;

                case CharacterState.Moving:
                    Move(totalMovementInput);
                    break;

                case CharacterState.Attacking:
                    break;

                case CharacterState.Dead:
                    break;
            }

            totalMovementInput = Vector2.Zero;
        }

        protected override void UpdateDirection()
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

        protected override void SetState(CharacterState newState)
        {
            if (State == newState) return; // Dont change the state to the same and reset the animation
            State = newState;

            // Something happens with the idle, it disappears for like a frame
            switch (State)
            {
                case CharacterState.Idle:
                    // Hands are stuck a little over the normal sprite
                    Animator.PlayAnimation(CharacterStateAnimations[State]);

                    SpriteRenderer.OriginOffSet = IdlespriteOffset;
                    break;

                case CharacterState.Moving:
                    // Hands are stuck a little over the normal sprite
                    Animator.PlayAnimation(CharacterStateAnimations[State]);

                    SpriteRenderer.OriginOffSet = LargeSpriteOffSet;
                    break;

                case CharacterState.Attacking:
                    // Is going to animate hands too.
                    Animator.PlayAnimation(CharacterStateAnimations[CharacterState.Idle]); // Just uses the Idle since we have no attacking animation

                    SpriteRenderer.OriginOffSet = IdlespriteOffset;
                    break;

                case CharacterState.Dead:
                    Animator.PlayAnimation(CharacterStateAnimations[State]);

                    SpriteRenderer.OriginOffSet = LargeSpriteOffSet;
                    Animator.StopCurrentAnimationAtLastSprite();
                    break;
            }
        }

        #region Item
        public void PickUpItem(GameObject item)
        {
            ItemInInventory = item.GetComponent<PickupableItem>();
        }

        public void UseItem()
        {
            if (ItemInInventory == null) return;

            ItemInInventory.Use();
        }
        #endregion

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
}