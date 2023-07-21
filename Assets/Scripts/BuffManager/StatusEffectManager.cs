using UnityEngine;

public abstract class StatusEffectManager
{
    public float durationSeconds;
    public int durationStacks;
    public float maxDuration;
    public int effectStacks;
    public StatusEffectObject buff { get; }
    protected readonly GameObject target;
    public bool IsFinished;

    public StatusEffectManager(StatusEffectObject _buff, GameObject _target)
    {
        buff = _buff;
        target = _target;
    }
    /**
     * Called every frame in Update, used for time baseds buff
     */
    public virtual void Tick(float delta)
    {
        if (buff.isTimeBased)
        {
            durationSeconds -= delta;
            if (durationSeconds <= 0)
            {
                Debug.Log(buff.name + "Tick end");
                End();
                IsFinished = true;
            }
        }
    }

    /**
     * Called when something happens, used for stack based buff
     */
    public virtual void Trigger(StatusEffectObject.TriggerType triggerType = default, GameObject target = null)
    {
        Debug.Log("Effect Triggered, Type: " + triggerType + "; to: " + target);
        if (triggerType == buff.thisTriggerType)
        {
            durationStacks -= 1;
            if (durationStacks <= 0)
            {
                Debug.Log(buff.name + "Trigger end");
                End();
                IsFinished = true;
            }
        }
    }

    /**
        * Activates buff or extends duration if ScriptableBuff has IsDurationStacked or IsEffectStacked set to true.
        */
    public void Activate()
    {
        if (buff.isTimeBased)
        {
            if (buff.effectMaxStack > effectStacks || durationSeconds <= 0)
            {
                ApplyEffect();
                effectStacks++;
            }

            if (buff.isDurationStacked || durationSeconds <= 0)
            {
                durationSeconds += buff.lifetime;
                maxDuration += buff.lifetime;
            } else if (buff.isDurationRefreshed)
            {
                durationSeconds = buff.lifetime;
                maxDuration = buff.lifetime;
            }
        } else
        {
            if (durationStacks < buff.durationMaxStack)
            {
                durationStacks = durationStacks + buff.stackDuration > buff.effectMaxStack ?
                    buff.effectMaxStack : durationStacks + buff.stackDuration;
            }
            if (effectStacks < buff.effectMaxStack)
            {
                ApplyEffect();
                effectStacks = effectStacks + buff.effectStack > buff.effectMaxStack ?
                    buff.effectMaxStack : effectStacks + buff.effectStack;
            }
        }
        
    }
    protected abstract void ApplyEffect();
    public abstract void End();
}
