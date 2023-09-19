using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum ItemType
{
    Default,
    AttackRune,
    SpellRune,
    StatusEffectRune,
    Relic
}

public enum Effect
{
    SpawnPrefab,
    AddStatusEffect,
}

public enum FollowUpTrigger
{
    EffectActive,
    EffectEnd,
    EffectHit,
    EffectBlock
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Item")]
public class ItemObject : ScriptableObject
{

    public Sprite uiDisplay;
    [TextArea(15, 20)]
    public string description;
    public Item data = new Item();

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }


}

[System.Serializable]
public class Item
{
    public string Name;
    public int Id = -1;
    public ItemType Type;
    public ItemEffect[] InitialEffects;
    public ItemEffect[] FollowUpEffects;
    public Item()
    {
        Name = "";
        Id = -1;
        InitialEffects = new ItemEffect[0];
        FollowUpEffects = new ItemEffect[0];
    }
    public Item(ItemObject item)
    {
        Name = item.data.Name;
        Id = item.data.Id;
        Type = item.data.Type;
        InitialEffects = item.data.InitialEffects;
        FollowUpEffects = item.data.FollowUpEffects;
    }
}

[System.Serializable]
public class ItemEffect
{
    public Effect effect;
    [ShowIf("effect", Effect.AddStatusEffect)]
    [AllowNesting]
    public StatusEffectObject statusEffect;

    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public GameObject effectPrefab;
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public GameObject indicator;
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public Vector3 scale;
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public bool IsAtSelf = true;
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float attackPercent; // percent of character attack this effect inherit
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float knockback; // knock back amount
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public int poiseBreak; // poise break amount
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float movement; // character movement after wind up
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float windUpTime; // wind up time before effect is active
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float duration; // duration of "effect"
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float windDownTime; // end of movement effect
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float speedDelta; // percent of speed change of character during windup + duration (-1 = can't move, -0.5 = reduce 50% movement speed)
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float lifetime = -1; // effect lifetime after instantiate, normally is windup + duration
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public float velocity; // velocity after init (for projectiles)
    [ShowIf("effect", Effect.SpawnPrefab)]
    [AllowNesting]
    public bool destroyOnHit; // for projectile


    public float energyDelta; // energy gain/consume amount

    public bool triggerNext; // casting this will trigger next attack as initial attack
    // follow up cannot trigger

    // NOT IMPLEMENTED
    public bool recastable; // some ability can be recasted once for additional function

    public float reloadDeltaCurrentWeapon; // added/subtracted reload time for this round
    // for the weapon this item is on
    public float reloadDeltaOtherWeapon; // added/subtracted reload time for this round
    // for the weapon the other weapon
    // follow up does not add reload time

    public bool IsFollowUp;
    [ShowIf("IsFollowUp")]
    [AllowNesting]
    public FollowUpTrigger followUpTrigger;
}