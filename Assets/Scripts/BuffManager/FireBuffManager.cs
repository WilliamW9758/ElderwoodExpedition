using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBuffManager : BuffManager
{
    private readonly EntityController controller;

    public FireBuffManager(Buff buff, GameObject target) : base(buff, target)
    {
        controller = target.GetComponent<EntityController>();
    }

    public override void Trigger(Buff.TriggerType triggerType = Buff.TriggerType.none, GameObject target = null)
    {
        if (triggerType == Buff.TriggerType.attackHit)
        {
            Debug.Log("Applied " + buff.name + " to " + target.name);
            FireBuff fireBuff = (FireBuff)buff;
            target.GetComponent<EntityController>().AddBuff(fireBuff.burnDebuff.InitializeBuff(target));
        }
        base.Trigger(triggerType, target);
    }

    protected override void ApplyEffect()
    {
        //Add speed increase to MovementComponent
        FireBuff fireBuff = (FireBuff) buff;
        controller.currentElement = EntityController.Elements.fire;
        controller.damageFlatMod += fireBuff.flatDamageDelta;
    }

    public override void End()
    {
        //Revert speed increase
        FireBuff fireBuff = (FireBuff)buff;
        controller.currentElement = EntityController.Elements.physical;
        controller.damageFlatMod -= fireBuff.flatDamageDelta * effectStacks;
        effectStacks = 0;
    }
}
