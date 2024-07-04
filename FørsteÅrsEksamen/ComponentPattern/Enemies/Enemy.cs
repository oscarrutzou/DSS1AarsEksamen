﻿using DoctorsDungeon.ComponentPattern.Path;
using DoctorsDungeon.ComponentPattern.PlayerClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoctorsDungeon.ComponentPattern.Enemies;

//Asser, Oscar
public abstract class Enemy : Character
{
    #region Properties
    private Grid grid;
    private Astar astar;
    private GameObject playerGo;
    public Player Player;
    private SpriteRenderer weaponSpriteRenderer;

    public List<GameObject> path { get; private set; }
    private Vector2 nextTarget;
    private Point targetPoint;
    private readonly float threshold = 10f;

    private bool hasBeenAwoken;

    #endregion Properties
    public Enemy(GameObject gameObject) : base(gameObject)
    {
        Speed = 250;
    }

    public override void Awake()
    {
        base.Awake();

        astar = new Astar();

        if (WeaponGo != null)
        {
            weaponSpriteRenderer = WeaponGo.GetComponent<SpriteRenderer>();  
        }

        Collider.SetCollisionBox(15, 27, new Vector2(0, 15));
    }

    public void SetStartPosition(GameObject playerGo, Point gridPos)
    {
        this.playerGo = playerGo;
        Player = playerGo.GetComponent<Player>();

        targetPoint = playerGo.Transform.GridPosition;
        grid = GridManager.Instance.CurrentGrid;
        GameObject.Transform.GridPosition = gridPos;
    }

    public override void Start()
    {
        astar.Start(GameObject);
        SpriteRenderer.SetLayerDepth(LayerDepth.EnemyUnder);

        SetState(CharacterState.Idle);

        // Sets start position
        GameObject currentCellGo = grid.GetCellGameObjectFromPoint(GameObject.Transform.GridPosition);
        GameObject.Transform.Position = currentCellGo.Transform.Position;
        Cell cell = currentCellGo.GetComponent<Cell>();
        CollisionNr = cell.CollisionNr;

        Weapon.MoveWeapon();

        if (Player.CollisionNr == CollisionNr) SetPath(); // We know that the player the targetPoint has been set
    }

    public override void Update(GameTime gameTime)
    {
        CheckLayerDepth(); // Make sure the enemy is drawn correctly.

        //To make a new path towards the player, if they have moved.
        PlayerMovedInRoom();

        switch (State)
        {
            case CharacterState.Idle:
                break;

            case CharacterState.Moving:
                UpdatePathing();
                break;

            case CharacterState.Attacking:

                // Do nothing if the player has died.
                if (Player.State == CharacterState.Dead) return;

                // Update direction towards player insted of moving direction
                Direction = Vector2.Normalize(playerGo.Transform.Position - GameObject.Transform.Position);
                Weapon.MoveWeapon();
                UpdateDirection();
                Attack();
                break;

            case CharacterState.Dead:
                break;
        }
    }

    private void PlayerMovedInRoom()
    {
        if (Player.CollisionNr != CollisionNr && !hasBeenAwoken || State == CharacterState.Dead) return; // Cant move if the player isnt in the same room.

        // Very precise but that is not needed in the start
        //if (playerGo.Transform.GridPosition != targetPoint)
        Point playerPos = playerGo.Transform.GridPosition;
        int precise = 3; 
        // If X is more that z cells away, it should start a new target. The same with Y
        if (Math.Abs(playerPos.X - targetPoint.X) >= precise ||
            Math.Abs(playerPos.Y - targetPoint.Y) >= precise)
        {
            targetPoint = playerGo.Transform.GridPosition;
            hasBeenAwoken = true;
            SetPath();
        }
    }

    private void CheckLayerDepth()
    {
        if (GameObject.Transform.Position.Y < playerGo.Transform.Position.Y)
        {
            SpriteRenderer.SetLayerDepth(LayerDepth.EnemyUnder);

            if (weaponSpriteRenderer == null) return;
            weaponSpriteRenderer.SetLayerDepth(LayerDepth.EnemyUnderWeapon);
        }
        else
        {
            SpriteRenderer.SetLayerDepth(LayerDepth.EnemyOver);

            if (weaponSpriteRenderer == null) return;
            weaponSpriteRenderer.SetLayerDepth(LayerDepth.EnemyOverWeapon);
        }
    }

    #region PathFinding
    Random rnd = new();
    private void SetPath()
    {
        ResetPathColor(); // For debugging

        path = null; // We cant use the previous path
        // Create a new thread to find the path 
        Thread thread = new(() =>
        {
            // Sleep for a little time, so the threads dont start at the same time
            Thread.Sleep(rnd.Next(1, 20)); 

            path = astar.FindPath(GameObject.Transform.GridPosition, targetPoint);
            if (State == CharacterState.Dead) // Bug happened because this path got returned just as it died
            {
                return;
            }
            if (path != null && path.Count > 0)
            {
                // If a new path is being set, set the next target to the enemy's current position
                SetState(CharacterState.Moving);
                if (GameObject.Transform.Position != path[0].Transform.Position)
                {
                    nextTarget = GameObject.Transform.Position;
                }
                else
                {
                    SetNextTargetPos(path[0]);
                }
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    private void UpdatePathing()
    {
        if (path == null)
            return;
        Vector2 position = GameObject.Transform.Position;

        if (Vector2.Distance(position, nextTarget) < threshold)
        {
            if (path.Count > 2)
            {
                GameObject.Transform.GridPosition = path[0].Transform.GridPosition;
                UpdateRoomNr(path[0]);
                ResetCellColor(path[0]);
                path.RemoveAt(0);
                SetNextTargetPos(path[0]);
            }
            else if (path.Count == 2) // Stops the path.
            {
                GameObject.Transform.GridPosition = path[0].Transform.GridPosition;
                UpdateRoomNr(path[0]);
                SetNextTargetPos(path[0]);
                ResetCellColor(path[0]);
                path.RemoveAt(0);
            }
        }

        Direction = Vector2.Normalize(nextTarget - position);

        GameObject.Transform.Translate(Direction * Speed * GameWorld.DeltaTime);
        Weapon.MoveWeapon();

        if (path.Count <= 3) // Attack before reaching the player, to put pressure on them
        {
            Direction = Vector2.Normalize(playerGo.Transform.Position - GameObject.Transform.Position);
            Attack();
        }

        if (path.Count == 1 && Vector2.Distance(position, nextTarget) < threshold)
        {
            SetState(CharacterState.Attacking); // Close so would always attack
    
            ResetCellColor(path[0]); // Debug

            path = null;
        }

        UpdateDirection();
    }

    private void UpdateRoomNr(GameObject cellGo)
    {
        Cell cell = cellGo.GetComponent<Cell>();
        CollisionNr = cell.CollisionNr;
    }

    private void ResetPathColor()
    {
        if (path != null)
        {
            foreach (GameObject cellGo in path)
            {
                ResetCellColor(cellGo);
            }
        }
    }

    private void ResetCellColor(GameObject cellGo)
    {
        Cell cell = cellGo.GetComponent<Cell>();
        cell.ChangeCellWalkalbeType(cell.CellWalkableType); // Dont change the type, but makes it draw the color of the debug, to the correct color.
    }

    private void SetNextTargetPos(GameObject cellGo)
    {
        nextTarget = cellGo.Transform.Position + new Vector2(0, -Cell.dimension * Cell.Scale / 2);
    }

    #endregion PathFinding
}