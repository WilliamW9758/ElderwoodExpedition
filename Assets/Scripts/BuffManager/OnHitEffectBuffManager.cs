using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitEffectBuffManager : StatusEffectManager
{
    private readonly EntityController controller;

    public OnHitEffectBuffManager(StatusEffectObject buff, GameObject target) : base(buff, target)
    {
        controller = target.GetComponent<EntityController>();
    }

    public override void Trigger(StatusEffectObject.TriggerType triggerType = StatusEffectObject.TriggerType.None, GameObject target = null)
    {
        if (triggerType == StatusEffectObject.TriggerType.OnHit)
        {
            OnHitEffectBuff onHitBuff = (OnHitEffectBuff)buff;
            target.GetComponent<EntityController>().AddStatusEffect(onHitBuff.onHitApplyDebuff.InitializeBuff(target));
        }
        base.Trigger(triggerType, target);
    }

    protected override void ApplyEffect()
    {
        OnHitEffectBuff onHitBuff = (OnHitEffectBuff) buff;
        controller.currentElement = onHitBuff.elementInfuse;
    }

    public override void End()
    {
        controller.currentElement = Elements.physical;
        effectStacks = 0;
    }
}
