using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBuffManager : BuffManager
{
    private readonly EntityController controller;

    public DamageBuffManager(Buff buff, GameObject target) : base(buff, target)
    {
        controller = target.GetComponent<EntityController>();
    }

    protected override void ApplyEffect()
    {
        DamageBuff damageBuff = (DamageBuff)buff;
        controller.damageFlatMod += damageBuff.damageDeltaFlat;
        controller.damageRatioMod += damageBuff.damageDeltaRatio;
    }

    public override void End()
    {
        //Revert speed increase
        DamageBuff damageBuff = (DamageBuff)buff;
        controller.damageFlatMod -= damageBuff.damageDeltaFlat * effectStacks;
        controller.damageRatioMod -= damageBuff.damageDeltaRatio * effectStacks;
        effectStacks = 0;
    }
}
