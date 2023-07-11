using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBuffManager : BuffManager
{
    private readonly EntityController controller;

    public SpeedBuffManager(Buff buff, GameObject target) : base(buff, target)
    {
        //Getting MovementComponent, replace with your own implementation
        controller = target.GetComponent<EntityController>();
    }

    protected override void ApplyEffect()
    {
        //Add speed increase to MovementComponent
        SpeedBuff speedBuff = (SpeedBuff) buff;
        controller.speed += speedBuff.speedDeltaFlat;
        controller.speedMod += speedBuff.speedDeltaRatio;
    }

    public override void End()
    {
        //Revert speed increase
        SpeedBuff speedBuff = (SpeedBuff) buff;
        controller.speed -= speedBuff.speedDeltaFlat * effectStacks;
        controller.speedMod -= speedBuff.speedDeltaRatio * effectStacks;
        effectStacks = 0;
    }
}
