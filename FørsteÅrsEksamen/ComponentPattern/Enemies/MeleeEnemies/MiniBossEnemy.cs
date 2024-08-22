﻿using DoctorsDungeon.ComponentPattern.Path;
using DoctorsDungeon.ComponentPattern.PlayerClasses;
using DoctorsDungeon.ComponentPattern.WorldObjects;
using DoctorsDungeon.Factory;
using DoctorsDungeon.GameManagement;
using DoctorsDungeon.GameManagement.Scenes.Rooms;
using DoctorsDungeon.Other;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DoctorsDungeon.ComponentPattern.Enemies.MeleeEnemies
{
    /*
     * Tribal Conjurer Lokar: 
     * Ancestral Caller Brug:
     * Spiritforge Garok:
     */
    public class MiniBossEnemy : EnemyMelee
    {
        private double spawnTimer;
        private double spawnCooldown = 5.0;

        private Spawner enemySpawner;
        private List<Enemy> spawnedEnemies = new();
        private List<Health> spawnedEnemiesHealth = new();

        private List<Point> points = new();
        private int maxAmountOfSpawns = 3;

        private RoomBase roomBase;

        public MiniBossEnemy(GameObject gameObject) : base(gameObject)
        {
            CanAttack = false;
        }  

        public void SetRoom(RoomBase roomBase) => this.roomBase = roomBase;

        public override void Awake()
        {
            base.Awake();

            Health.SetHealth(400);

            CharacterStateAnimations.Add(CharacterState.Idle, AnimNames.OrcShamanIdle);
            CharacterStateAnimations.Add(CharacterState.Moving, AnimNames.OrcShamanRun);
            CharacterStateAnimations.Add(CharacterState.Dead, AnimNames.OrcShamanDeath);

            MakeSpawner();
        }

        private void MakeSpawner()
        {
            GameObject spawnerGo = new();
            enemySpawner = spawnerGo.AddComponent<Spawner>();
        }

        public override void Attack()
        {
            /*
             * Boss: If hit, hit back if possible.
             * Boss: Not getting hit and spawn timer is down, spawn enemy.
             */
            //SpawnEnemy();

            // Attack with normal
        }

        public override void Update()
        {
            base.Update();

            if (spawnTimer < spawnCooldown)
            {
                spawnTimer += GameWorld.DeltaTime;
            }

            if (Health.IsDead) return;
            SpawnEnemy();
        }
        private void SpawnEnemy()
        {
            if (spawnTimer < spawnCooldown) return;

            // Make this cheaper to run, not as often.
            spawnedEnemiesHealth.Clear();

            foreach (Enemy enemy in spawnedEnemies)
            {
                Health health = enemy.GameObject.GetComponent<Health>();
                spawnedEnemiesHealth.Add(health);
            }
            // Finds all enemies that arent dead
            spawnedEnemiesHealth = spawnedEnemiesHealth.FindAll(x => !x.IsDead);

            if (spawnedEnemiesHealth.Count >= maxAmountOfSpawns) return;

            spawnTimer = 0;

            // Spawn enemy at location.
            points.Clear();

            // Select a random point

            // Amount to spawn
            points.Add(GameObject.Transform.GridPosition);

            List<Enemy> newEnemies = enemySpawner.SpawnEnemies(points, Player.GameObject, spawnAbleTypes);

            roomBase.EnemiesInRoom.AddRange(newEnemies);

            spawnedEnemies.AddRange(newEnemies);

            foreach (Enemy enemy in spawnedEnemies)
            {
                enemy.Astar.SetEnemyListReferences(spawnedEnemies);
                enemy.HasBeenAwoken = true;
            }
        }

        private List<EnemyTypes> spawnAbleTypes = new List<EnemyTypes>()
        {
            EnemyTypes.OrcArcher,
            EnemyTypes.OrcWarrior,
        };

    }
}
