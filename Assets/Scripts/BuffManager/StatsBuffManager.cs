using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsBuffManager : StatusEffectManager
{
    private readonly EntityController controller;

    public StatsBuffManager(StatusEffectObject buff, GameObject target) : base(buff, target)
    {
        controller = target.GetComponent<EntityController>();
    }

    protected override void ApplyEffect()
    {
        StatsBuff statsBuff = (StatsBuff)buff;
        controller.speed += statsBuff.speedDeltaFlat;
        controller.speedMod += statsBuff.speedDeltaRatio;
        controller.maxHealth += statsBuff.maxHealthDelta;
        controller.critRate += statsBuff.critRateDelta;
        controller.critDamage += statsBuff.critDamageDelta;
        controller.attackFlatMod += statsBuff.damageDeltaFlat;
        controller.attackRatioMod += statsBuff.damgeDeltaRatio;
    }

    public override void End()
    {
        //Revert speed increase
        StatsBuff statsBuff = (StatsBuff)buff;
        controller.speed -= statsBuff.speedDeltaFlat * effectStacks;
        controller.speedMod -= statsBuff.speedDeltaRatio * effectStacks;
        controller.maxHealth -= statsBuff.maxHealthDelta * effectStacks;
        controller.critRate -= statsBuff.critRateDelta * effectStacks;
        controller.critDamage -= statsBuff.critDamageDelta * effectStacks;
        controller.attackFlatMod -= statsBuff.damageDeltaFlat * effectStacks;
        controller.attackRatioMod -= statsBuff.damgeDeltaRatio * effectStacks;
        effectStacks = 0;
    }
}
