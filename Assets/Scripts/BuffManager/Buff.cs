using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

public abstract class Buff : ScriptableObject
{
    public string description;
    public bool isTimeBased;
    private bool isStack => !isTimeBased;
    [ShowIf("isTimeBased")]
    public float lifetime;
    [ShowIf("isStack")]
    public int stackDuration;
    [ShowIf("isStack")]
    public int durationMaxStack;
    public int effectStack;
    public int effectMaxStack;
    [ShowIf("isTimeBased")]
    public bool isDurationStacked;
    [ShowIf("isTimeBased")]
    public bool isDurationRefreshed;
    public bool canBeCleansed;
    public bool isBuff;
    public Sprite icon;

    public enum TriggerType
    {
        none,
        anyCast,
        spellCast,
        attackCast,
        attackHit,
        attackEnd,
        spellEnd,
        attackKill,
        getHit,
        takeDamage,
    }
    [ShowIf("isStack")]
    public TriggerType thisTriggerType;

    //public int flatDamage;
    //public float damageMod;
    //public float speedMod;
    //public int healthMod; // negative as damage
    //public float healthModTickRate; // heal/damage every X seconds
    //public float critRate;
    //public float critDamage;

    public abstract BuffManager InitializeBuff(GameObject obj);
}
