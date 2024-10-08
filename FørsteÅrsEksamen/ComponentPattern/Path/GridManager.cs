﻿using ShamansDungeon.CommandPattern;
using ShamansDungeon.ComponentPattern.GUI;
using ShamansDungeon.LiteDB;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ShamansDungeon.ComponentPattern.Path;

public enum DrawMapSelecter
{
    DrawRoomColliders,
    DrawBlackedOutRooms,
}

public class GridManager
{
    #region Parameters

    private static GridManager instance;

    public static GridManager Instance
    { get { return instance ??= instance = new GridManager(); } }

    private Grid currentGrid;

    public Grid CurrentGrid
    {
        get { return currentGrid; }
        private set
        {
            if (value != currentGrid)
            {
                currentGrid = value;
            }
        }
    }

    private int _colliderNrIndex = 1;
    private int _roomNrIndex = 1;

    /// <summary>
    /// For the room index on the cells, right now there is a limit of 10 rooms.
    /// </summary>
    public int ColliderNrIndex
    {
        get { return _colliderNrIndex; }
        set
        {
            if (value == 0 || value > 10) return;

            if (_colliderNrIndex != value)
            {
                _colliderNrIndex = value;
            }
        }
    }

    public int RoomNrIndex
    {
        get { return _roomNrIndex; }
        set
        {
            if (value == 0 || value > 10) return;

            if (_roomNrIndex != value)
            {
                _roomNrIndex = value;
            }
        }
    }

    public DrawMapSelecter CurrentDrawSelected { get; set; } = DrawMapSelecter.DrawBlackedOutRooms;

    public List<int> PlayerDiscoveredRoomNmbers = new();

    #endregion Parameters

    public void ResetGridManager()
    {
        // Make a new list, and populate 1, so the player can see the first room.
        PlayerDiscoveredRoomNmbers = new() { 1 };
    }

    public void ChangeNumberIndex(int addToNumber)
    {
        if (!InputHandler.Instance.DebugMode) return;

        if (CurrentDrawSelected == DrawMapSelecter.DrawRoomColliders)
        {
            ColliderNrIndex += addToNumber;
        }
        else if (CurrentDrawSelected == DrawMapSelecter.DrawBlackedOutRooms)
        {
            RoomNrIndex += addToNumber;
        }
    }

    public void AddVisitedRoomNumbers(int roomNr)
    {
        if (!PlayerDiscoveredRoomNmbers.Contains(roomNr))
        {
            PlayerDiscoveredRoomNmbers.Add(roomNr);
            ShouldDrawCells();
        }
    }

    public void SaveLoadGrid(Grid grid)
    {
        GameObject savedGrid = DB.Instance.LoadGrid(grid.Name);

        if (savedGrid == null) // No saved
        {
            DB.Instance.SaveGrid(grid);
            CurrentGrid = grid;
        }
        else
        {
            CurrentGrid = savedGrid.GetComponent<Grid>();
        }
    }

    #region Draw and Remove Current Grid

    public void DrawOnCells()
    {
        if (!InputHandler.Instance.DebugMode
            || GuiMethods.IsMouseOverUI()) return;

        GameObject cellGo = GetCellAtPos(InputHandler.Instance.MouseInWorld);
        if (cellGo == null) return;

        if (CurrentDrawSelected == DrawMapSelecter.DrawRoomColliders)
        {
            SetCollisionCellProperties(cellGo, CellWalkableType.FullValid, ColliderNrIndex); // Move the is walkable out of this
        }
        else if (CurrentDrawSelected == DrawMapSelecter.DrawBlackedOutRooms)
        {
            SetRoomCellProperties(cellGo, RoomNrIndex);
        }
    }

    public void SetDefaultOnCell()
    {
        if (!InputHandler.Instance.DebugMode
            || GuiMethods.IsMouseOverUI()) return;

        GameObject cellGo = GetCellAtPos(InputHandler.Instance.MouseInWorld);
        if (cellGo == null) return;

        if (CurrentDrawSelected == DrawMapSelecter.DrawRoomColliders)
        {
            SetCollisionCellProperties(cellGo, CellWalkableType.NotValid, -1);
        }
        else if (CurrentDrawSelected == DrawMapSelecter.DrawBlackedOutRooms)
        {
            SetRoomCellProperties(cellGo, -1);
        }
    }

    public void ChangeSelectedDraw(DrawMapSelecter selected)
    {
        CurrentDrawSelected = selected;

        ShouldDrawCells();
    }

    private void SetCollisionCellProperties(GameObject cellGo, CellWalkableType walkableType, int collisionNr)
    {
        Cell cell = cellGo.GetComponent<Cell>();
        cell.CollisionNr = collisionNr;
        cell.ChangeCellWalkalbeType(walkableType);
    }

    private void SetRoomCellProperties(GameObject cellGo, int roomNr)
    {
        Cell cell = cellGo.GetComponent<Cell>();
        cell.RoomNr = roomNr;

        CheckCellIsDiscovered(cell);
    }

    public void DeleteDrawnGrid()
    {
        if (CurrentGrid == null) return;

        foreach (GameObject cellGo in CurrentGrid.Cells.Values)
        {
            GameWorld.Instance.Destroy(cellGo);
        }
        GameWorld.Instance.Destroy(CurrentGrid.GameObject);
        CurrentGrid = null;
    }

    #endregion Draw and Remove Current Grid

    #region Return Methods

    public GameObject GetCellAtPos(Vector2 pos)
    {
        if (CurrentGrid == null) return null;

        GameObject go = CurrentGrid.GetCellGameObject(pos);
        if (go != null)
        {
            return go;
        }

        return null;
    }

    public Vector2 GetCornerPositionOfCell(Point gridCell)
    {
        Vector2 temp = Vector2.Zero;
        if (CurrentGrid == null) return temp;

        temp = CurrentGrid.PosFromGridPos(gridCell);
        temp -= new Vector2(Cell.Dimension * Cell.Scale / 2, Cell.Dimension * Cell.Scale / 2);

        return temp;
    }

    public void ShowHideGrid()
    {
        InputHandler.Instance.DebugMode = !InputHandler.Instance.DebugMode;

        if (!InputHandler.Instance.DebugMode) return;

        ShouldDrawCells();
    }

    private void ShouldDrawCells()
    {
        if (CurrentGrid == null) return;

        foreach (GameObject go in CurrentGrid.Cells.Values)
        {
            Cell cell = go.GetComponent<Cell>();

            if (CurrentDrawSelected == DrawMapSelecter.DrawRoomColliders)
            {
                if (InputHandler.Instance.DebugMode)
                {
                    if (cell.CollisionNr == -1) go.IsEnabled = false;
                    else go.IsEnabled = true;
                }
            }
            else if (CurrentDrawSelected == DrawMapSelecter.DrawBlackedOutRooms)
            {
                
                CheckCellIsDiscovered(cell);
            }
        }
    }

    public void SetCellsVisibility()
    {
        if (CurrentGrid == null) throw new System.Exception("You have to have a grid to change the cells");

        foreach (GameObject go in CurrentGrid.Cells.Values)
        {
            Cell cell = go.GetComponent<Cell>();

            CheckCellIsDiscovered(cell);
        }
    }

    private void CheckCellIsDiscovered(Cell cell)
    {
        GameObject go = cell.GameObject;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

        if (InputHandler.Instance.DebugMode)
        {
            // Show the cells
            go.IsEnabled = true;
            sr.ShouldDrawSprite = true;
            sr.Color = new Color(0, 0, 0, 125);

            return;
        }

        // If the player already have unloacked the room, or if the room nr have not been set
        if (PlayerDiscoveredRoomNmbers.Contains(cell.RoomNr) || cell.RoomNr == -1)
        {
            go.IsEnabled = false;
            sr.ShouldDrawSprite = false;

            // Should start a bool that begins a lerp of the sr.color to Transparent, and then sets the -
            // IsEnabled and ShouldDrawSprite to false
        }
        else
        {
            go.IsEnabled = true;
            sr.ShouldDrawSprite = true;
            sr.Color = cell.NotDiscoveredColor;
        }
    }

    

    #endregion Return Methods
}