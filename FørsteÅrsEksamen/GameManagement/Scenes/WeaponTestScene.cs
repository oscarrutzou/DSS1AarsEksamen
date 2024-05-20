﻿using FørsteÅrsEksamen.CommandPattern;
using FørsteÅrsEksamen.CommandPattern.Commands;
using FørsteÅrsEksamen.ComponentPattern;
using FørsteÅrsEksamen.ComponentPattern.Classes;
using FørsteÅrsEksamen.ComponentPattern.Weapons;
using FørsteÅrsEksamen.Factory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FørsteÅrsEksamen.GameManagement.Scenes
{
    internal class WeaponTestScene : Scene
    {
        public override void Initialize()
        {
            MakePlayer();

            SetCommands();
        }

        private void Attack()
        {
            Player player = playerGo.GetComponent<Player>();
            Weapon weapon = player.WeaponGo.GetComponent<Weapon>();
            weapon.Attack();
        }

        private GameObject playerGo;

        private void MakePlayer()
        {
            playerGo = PlayerFactory.Create(ClassTypes.Warrior, WeaponTypes.Sword);
            GameWorld.Instance.WorldCam.position = playerGo.Transform.Position;
            GameWorld.Instance.Instantiate(playerGo);
        }

        private Player player;

        private void SetCommands()
        {
            player = playerGo.GetComponent<Player>();
            InputHandler.Instance.AddKeyUpdateCommand(Keys.D, new MoveCmd(player, new Vector2(1, 0)));
            InputHandler.Instance.AddKeyUpdateCommand(Keys.A, new MoveCmd(player, new Vector2(-1, 0)));
            InputHandler.Instance.AddKeyUpdateCommand(Keys.W, new MoveCmd(player, new Vector2(0, -1)));
            InputHandler.Instance.AddKeyUpdateCommand(Keys.S, new MoveCmd(player, new Vector2(0, 1)));

            InputHandler.Instance.AddKeyButtonDownCommand(Keys.Space, new CustomCmd(Attack));
        }

        private void TestRemoveComm()
        {
            InputHandler.Instance.RemoveKeyUpdateCommand(Keys.S);
        }
    }
}