﻿using ShamansDungeon.GameManagement;

namespace ShamansDungeon.ComponentPattern.Enemies.MeleeEnemies;

//Asser

public class OrcWarrior : EnemyMelee
{
    public OrcWarrior(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Awake()
    {
        base.Awake();

        Health.SetHealth(200);

        CharacterStateAnimations.Add(CharacterState.Idle, AnimNames.OrcWarriorIdle);
        CharacterStateAnimations.Add(CharacterState.Moving, AnimNames.OrcWarriorRun);
        CharacterStateAnimations.Add(CharacterState.Dead, AnimNames.OrcWarriorDeath);
    }
}