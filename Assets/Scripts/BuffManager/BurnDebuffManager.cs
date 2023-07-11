using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnDebuffManager : BuffManager
{
    private readonly EntityController controller;
    private float burnTimer;

    public BurnDebuffManager(Buff buff, GameObject target) : base(buff, target)
    {
        controller = target.GetComponent<EntityController>();
    }

    public override void Tick(float delta)
    {
        BurnDebuff burnDebuff = (BurnDebuff)buff;
        Debug.Log("Burn tick, current burn Timer: " + burnTimer);
        burnTimer -= delta;
        if (burnTimer <= 0)
        {
            Debug.Log("Burn tick trigger");
            controller.TakeDamage(burnDebuff.burnDamage * effectStacks, 0, EntityController.Elements.fire);
            burnTimer = burnDebuff.tickRate;
        }
        base.Tick(delta);
    }

    protected override void ApplyEffect()
    {
        BurnDebuff burnDebuff = (BurnDebuff)buff;
        burnTimer = burnDebuff.tickRate;
    }

    public override void End()
    {
        BurnDebuff burnDebuff = (BurnDebuff)buff;
        if (burnTimer < 0.05f)
        {
            controller.TakeDamage(burnDebuff.burnDamage * effectStacks, 0, EntityController.Elements.fire);
        }
    }
}
