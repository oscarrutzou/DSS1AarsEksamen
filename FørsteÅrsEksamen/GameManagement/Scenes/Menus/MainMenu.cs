﻿using FørsteÅrsEksamen.CommandPattern;
using FørsteÅrsEksamen.CommandPattern.Commands;
using FørsteÅrsEksamen.ComponentPattern;
using FørsteÅrsEksamen.ComponentPattern.GUI;
using FørsteÅrsEksamen.Factory.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FørsteÅrsEksamen.GameManagement.Scenes.Menus
{
    public class MainMenu : Scene
    {
        private bool showSettings;

        private List<GameObject> startMenuObjects;
        private List<GameObject> pauseMenuObjects;

        private SpriteFont font;
        private Vector2 textPos;
        private Button musicBtn, sfxBtn;
        
        public override void Initialize()
        {
            startMenuObjects = new();
            pauseMenuObjects = new();
            showSettings = false;

            GlobalSounds.InMenu = true;

            font = GlobalTextures.BigFont;
            textPos = GameWorld.Instance.UiCam.Center + new Vector2(0, -200);
            
            //Draw background

            InitStartMenu();
            InitPauseMenu();
        }

        private void InitStartMenu()
        {
            GameObject startBtn = ButtonFactory.Create("Start Game", true,
                () => { GameWorld.Instance.ChangeRoom(0); });
                //() => { GameWorld.Instance.ChangeScene(ScenesNames.OscarTestScene); });
            startMenuObjects.Add(startBtn);

            GameObject settingsBtn = ButtonFactory.Create("Settings", true,
                 () => { Settings(); });
            startMenuObjects.Add(settingsBtn);
            
            GameObject quitBtn = ButtonFactory.Create("Quit", true,
                 () => { GameWorld.Instance.Exit(); });
            startMenuObjects.Add(quitBtn);

            GuiMethods.PlaceButtons(startMenuObjects, textPos + new Vector2(0, 75), 25);
        }

        private void InitPauseMenu()
        {
            GameObject musicVolGo = ButtonFactory.Create("", true, () => { ChangeMusic(); });
            musicBtn = musicVolGo.GetComponent<Button>();
            musicBtn.ChangeScale(new Vector2(14, 4));
            musicBtn.Text = $"Music Volume {GlobalSounds.MusicVolume * 100}%";
            pauseMenuObjects.Add(musicVolGo);

            GameObject sfxVolGo = ButtonFactory.Create("", true, () => { ChangeSfx(); });
            sfxBtn = sfxVolGo.GetComponent<Button>();
            sfxBtn.ChangeScale(new Vector2(14, 4));
            sfxBtn.Text = $"SFX Volume {GlobalSounds.SfxVolume * 100}%";
            pauseMenuObjects.Add(sfxVolGo);

            GameObject quitBtn = ButtonFactory.Create("Back", true, () => { Settings(); });
            pauseMenuObjects.Add(quitBtn);

            ShowHideGameObjects(pauseMenuObjects, false);

            GuiMethods.PlaceButtons(pauseMenuObjects, textPos + new Vector2(0, 75), 25);
        }

        private void ChangeMusic()
        {
            GlobalSounds.ChangeMusicVolume();
            musicBtn.Text = $"Music Volume {GlobalSounds.MusicVolume * 100}%";
        }

        private void ChangeSfx()
        {
            GlobalSounds.ChangeSfxVolume();
            sfxBtn.Text = $"SFX Volume {GlobalSounds.SfxVolume * 100}%";
        }

        private void Settings()
        {
            showSettings = !showSettings;

            if (showSettings) {
                ShowHideGameObjects(startMenuObjects, false);
                ShowHideGameObjects(pauseMenuObjects, true);
            }
            else
            {
                ShowHideGameObjects(startMenuObjects, true);
                ShowHideGameObjects(pauseMenuObjects, false);
            }
        }

        private void ShowHideGameObjects(List<GameObject> gameObjects, bool isEnabled)
        {
            foreach (GameObject item in gameObjects)
            {
                item.IsEnabled = isEnabled;
            }
        }

        public override void DrawOnScreen(SpriteBatch spriteBatch)
        {
            base.DrawOnScreen(spriteBatch);

            if (showSettings)
            {
                DrawMenuText(spriteBatch, "Settings", textPos);
            }
            else
            {
                DrawMenuText(spriteBatch, "Doctor's Dunguon", textPos);
            }
        }

        private void DrawMenuText(SpriteBatch spriteBatch, string text, Vector2 position)
        {
            GuiMethods.DrawTextCentered(spriteBatch, font, position, text, Color.Black);
        }


    }
}