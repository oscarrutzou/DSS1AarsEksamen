﻿using DoctorsDungeon.ComponentPattern;
using DoctorsDungeon.Factory;

namespace DoctorsDungeon.ObjectPoolPattern;

// Stefan
public class ProjectilePool : ObjectPool
{
    private static ProjectilePool _instance;

    public static ProjectilePool Instance
    { get { return _instance ??= new ProjectilePool(); } }

    public override void CleanUp(GameObject gameObject)
    {
    }

    public override GameObject CreateObject(params object[] args)
    {
        return ProjectileFactory.Create();
    }
}