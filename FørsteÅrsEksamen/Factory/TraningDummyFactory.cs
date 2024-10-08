﻿using ShamansDungeon.ComponentPattern;
using ShamansDungeon.ComponentPattern.WorldObjects;
using Microsoft.Xna.Framework;

namespace ShamansDungeon.Factory
{
    public static class TraningDummyFactory
    {
        public static GameObject Create()
        {
            GameObject go = new GameObject();
            go.Type = GameObjectTypes.Enemy;
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<Animator>();
            go.AddComponent<TrainingDummy>();
            go.AddComponent<Health>();
            go.AddComponent<Collider>(); 

            GameWorld.Instance.Instantiate(go);

            return go;
        }

    }
}
