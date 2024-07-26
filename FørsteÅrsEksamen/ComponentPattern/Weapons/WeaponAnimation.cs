﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorsDungeon.ComponentPattern.Weapons;

public enum WeaponAnimTypes
{
    Light,
    Medium,
    Heavy,
    // Maybe special or something
}
// A weapon will always have a light attack. It goes though the enum, enum.default = first in.
// Need to save how many that attack has been picked, with how many times it should repeat
// Could do it with a int that

public delegate float AnimationFunction(float x);

// Play with some other methods, for different weapons, to make them feel slow or fast https://easings.net/

public class WeaponAnimation
{
    public float TotalTime;
    public float AmountOfRotation;
    public int Repeats;
    public int Damage;
    public AnimationFunction AnimationMethod; // Delegate field
    public WeaponAnimTypes NextAnimation;

    public WeaponAnimation(float totalTime,
                           float amountOfRotation,
                           int damage,
                           AnimationFunction animationMethod,
                           WeaponAnimTypes nextAnimation,
                           int repeats = 1)
    {
        TotalTime = totalTime;
        AmountOfRotation = amountOfRotation;
        Damage = damage;
        AnimationMethod = animationMethod; // Assign the delegate
        NextAnimation = nextAnimation;
        Repeats = repeats;
    }

    public float CalculateAnimation(float x)
    {
        return AnimationMethod(x);
    }
}
