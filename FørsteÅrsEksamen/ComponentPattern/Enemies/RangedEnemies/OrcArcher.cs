﻿//using FørsteÅrsEksamen.ComponentPattern.Characters;
using DoctorsDungeon.ComponentPattern.Enemies.MeleeEnemies;
using DoctorsDungeon.GameManagement;

namespace DoctorsDungeon.ComponentPattern.Enemies.RangedEnemies;

//Asser

public class OrcArcher : EnemyMelee
{
    public OrcArcher(GameObject gameObject) : base(gameObject)
    {
        Speed = 350;
    }

    public override void Awake()
    {
        base.Awake();

        Health.SetHealth(150);

        CharacterStateAnimations.Add(CharacterState.Idle, AnimNames.OrcArcherIdle);
        CharacterStateAnimations.Add(CharacterState.Moving, AnimNames.OrcArcherRun);
        CharacterStateAnimations.Add(CharacterState.Dead, AnimNames.OrcArcherDeath);
    }
}