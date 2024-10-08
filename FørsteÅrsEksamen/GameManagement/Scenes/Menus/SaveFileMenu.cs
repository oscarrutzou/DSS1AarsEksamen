﻿using ShamansDungeon.ComponentPattern;
using ShamansDungeon.ComponentPattern.GUI;
using ShamansDungeon.Factory.Gui;
using ShamansDungeon.LiteDB;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ShamansDungeon.GameManagement.Scenes.Menus;

// Erik
public class SaveFileMenu : MenuScene
{
    private Dictionary<int, Button> _saveFileButtons;
    private readonly string _newSaveFile = "New Save";

    protected override void InitFirstMenu()
    {
        SaveData.SetBaseValues();

        _saveFileButtons = new Dictionary<int, Button>()
        {
            { 1, ButtonFactory.Create(_newSaveFile, true, () => { MakeNewSaveFile(1); }, TextureNames.WideBtn).GetComponent<Button>() },
            { 2, ButtonFactory.Create(_newSaveFile, true, () => { MakeNewSaveFile(2); }, TextureNames.WideBtn).GetComponent<Button>()  },
            { 3, ButtonFactory.Create(_newSaveFile, true, () => { MakeNewSaveFile(3); }, TextureNames.WideBtn).GetComponent<Button>()  }
        };

        foreach (Button button in _saveFileButtons.Values)
        {
            FirstMenuObjects.Add(button.GameObject);
        }

        GameObject backBtn = ButtonFactory.Create("Back", true, () =>
        {
            GameWorld.Instance.ChangeScene(SceneNames.MainMenu);
        });

        FirstMenuObjects.Add(backBtn);
    }

    public override void AfterFirstCleanUp()
    {
        GuiMethods.PlaceGameObjectsVertical(FirstMenuObjects, TextPos + new Vector2(0, 75), 25);
        ChangeButtonText();
    }

    private void MakeNewSaveFile(int id)
    {
        SaveData.SetBaseValues();

        SaveData.CurrentSaveID = id;

        SaveFileData saveFile = DB.Instance.LoadGame();

        saveFile ??= DB.Instance.SaveGame(id); // Makes the save id

        if (saveFile.RunData == null)
            // Make a new run after the characterSelector
            GameWorld.Instance.ChangeScene(SceneNames.CharacterSelectorMenu);
        else
        {
            SaveData.Time_Left = saveFile.RunData.Time_Left;
            // Loads run
            GameWorld.Instance.ChangeDungeonScene(SceneNames.DungeonRoom, saveFile.RunData.Room_Reached);
        }
    }

    /// <summary>
    /// Updates the Button text
    /// </summary>
    private void ChangeButtonText()
    {
        List<SaveFileData> saveFiles = DB.Instance.LoadAllSaveFiles();

        if (saveFiles.Count == 0) return; // There is no files yet, so we dont change the text.

        foreach (SaveFileData saveFile in saveFiles)
        {
            if (!_saveFileButtons.ContainsKey(saveFile.Save_ID)) continue;

            Button saveFileBtn = _saveFileButtons[saveFile.Save_ID];

            saveFileBtn.Text =
                    $"Save {saveFile.Save_ID}" +
                    $"\nCurrency {saveFile.Currency}"; // Removes .ToString+$"\n Last Login {saveFile.Last_Login:MM-dd}"

            // Add a delete button next to it.
            GameObject deleteBtn = ButtonFactory.Create("X", true, () => { DeleteSave(saveFile.Save_ID); }, TextureNames.DeleteSaveBtn);
            Button delete = deleteBtn.GetComponent<Button>();

            deleteBtn.Transform.Position = saveFileBtn.GameObject.Transform.Position + new Vector2(150, 0);
            GameWorld.Instance.Instantiate(deleteBtn);
        }
    }

    private void DeleteSave(int saveID)
    {
        DB.Instance.DeleteSave(saveID);

        GameWorld.Instance.ChangeScene(SceneNames.SaveFileMenu);
    }
}