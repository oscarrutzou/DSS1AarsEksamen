﻿using DoctorsDungeon.CommandPattern;
using DoctorsDungeon.CommandPattern.Commands;
using DoctorsDungeon.ComponentPattern;
using DoctorsDungeon.ComponentPattern.Path;
using DoctorsDungeon.ComponentPattern.PlayerClasses;
using DoctorsDungeon.ComponentPattern.Weapons;
using DoctorsDungeon.ComponentPattern.Weapons.RangedWeapons;
using DoctorsDungeon.Factory;
using DoctorsDungeon.LiteDB;
using DoctorsDungeon.Other;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace DoctorsDungeon.GameManagement.Scenes.TestScenes;

public class ErikTestScene : Scene
{
    private GameObject weapon, bow, PlayerGo, spawnerGameObject;
    private Point PlayerSpawnPos;
    private Player player;

    public override void Initialize()
    {
        SetLevelBG();
        StartGrid();
        PlayerSpawnPos = new Point(5, 5);
        MakePlayer();

        OnPlayerChanged();

        //InitSpawner();

        MakeWeapon();
        GameWorld.Instance.WorldCam.Position = Vector2.Zero;

        AttackCommand();
        //spawner = new Spawner(new GameObject(), player);
        //GameWorld.Instance.Instantiate(spawner.GameObject);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    private List<Point> spawnPoints = new()
    {
        new Point(5, 5),
        new Point(7, 5),
        new Point(7, 7),
    };

    private void InitSpawner()
    {
        spawnerGameObject = new GameObject();
        Spawner spawner = spawnerGameObject.AddComponent<Spawner>();
        spawner.SpawnEnemies(spawnPoints, PlayerGo);
    }

    private void SetLevelBG()
    {
        GameObject go = new();
        go.Type = GameObjectTypes.Background;

        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.SetSprite(TextureNames.TestLevelBG);
        spriteRenderer.IsCentered = false;

        GameWorld.Instance.Instantiate(go);
    }

    private void MakePlayer()
    {
        PlayerGo = PlayerFactory.Create(ClassTypes.Warrior, WeaponTypes.Sword);
        PlayerGo.Transform.Position = GridManager.Instance.CurrentGrid.Cells[PlayerSpawnPos].Transform.Position;
        PlayerGo.Transform.GridPosition = PlayerSpawnPos;
        GameWorld.Instance.WorldCam.Position = PlayerGo.Transform.Position;
        GameWorld.Instance.Instantiate(PlayerGo);
    }

    private void StartGrid()
    {
        GameObject gridGo = new();
        Grid grid = gridGo.AddComponent<Grid>("Test1", new Vector2(0, 0), 24, 18);
        grid.GenerateGrid();
        GridManager.Instance.SaveLoadGrid(grid);
    }

    private void SetCommands()
    {
        player = PlayerGo.GetComponent<Player>();

        InputHandler.Instance.AddKeyUpdateCommand(Keys.D, new MoveCmd(player, new Vector2(1, 0)));
        InputHandler.Instance.AddKeyUpdateCommand(Keys.A, new MoveCmd(player, new Vector2(-1, 0)));
        InputHandler.Instance.AddKeyUpdateCommand(Keys.W, new MoveCmd(player, new Vector2(0, -1)));
        InputHandler.Instance.AddKeyUpdateCommand(Keys.S, new MoveCmd(player, new Vector2(0, 1)));

        InputHandler.Instance.AddKeyButtonDownCommand(Keys.D1, new CustomCmd(player.UseItem));
        InputHandler.Instance.AddKeyButtonDownCommand(Keys.Tab, new CustomCmd(() => { GridManager.Instance.ShowHideGrid(); }));
        InputHandler.Instance.AddKeyButtonDownCommand(Keys.Space, new CustomCmd(Attack));
        InputHandler.Instance.AddKeyButtonDownCommand(Keys.O, new CustomCmd(() => { DB.Instance.SaveGrid(GridManager.Instance.CurrentGrid); }));
    }

    public override void OnPlayerChanged()
    {
        InputHandler.Instance.RemoveAllExeptBaseCommands();
        SetCommands();
    }

    private void MakeWeapon()
    {
        weapon = WeaponFactory.Create(WeaponTypes.Sword);

        bow = WeaponFactory.Create(WeaponTypes.Bow);

        GameWorld.Instance.Instantiate(weapon);
        GameWorld.Instance.Instantiate(bow);
    }

    private void Attack()
    {
        weapon.GetComponent<Weapon>().StartAttack();

        //projectile.GetComponent<MagicStaff>().Attack();
    }

    private void AttackCommand()
    {
        InputHandler.Instance.AddKeyButtonDownCommand(Keys.Space,
            new CustomCmd(Attack));

        InputHandler.Instance.AddKeyButtonDownCommand(Keys.B,
            new CustomCmd(Shoot));
    }

    public void Shoot()
    {
        var rangedWeapon = bow.GetComponent<RangedWeapon>();

        //projectile.GetComponent<Projectile>().SetValues(MathHelper.Pi);

        if (rangedWeapon != null)
        {
            rangedWeapon.Shoot();
        }
    }

    public override void DrawInWorld(SpriteBatch spriteBatch)
    {
        base.DrawInWorld(spriteBatch);

        //spriteBatch.Draw(GlobalTextures.Textures[TextureNames.WoodSword], Vector2.Zero, Color.White);
    }

    public override void DrawOnScreen(SpriteBatch spriteBatch)
    {
        base.DrawOnScreen(spriteBatch);
    }
}