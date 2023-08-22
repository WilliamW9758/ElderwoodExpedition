using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EffectManager : MonoBehaviour
{
    public UnityAction<EffectManager, ItemEffect, GameObject> OnEffectActive;
    public UnityAction<EffectManager, ItemEffect, GameObject> OnEffectEnd;
    public UnityAction<EffectManager, ItemEffect, GameObject> OnEffectHit;
    public UnityAction<EffectManager, ItemEffect, GameObject> OnEffectBlock;
    public List<ItemEffect> attachedFollowUpEffects;

    public float attackPercent; // percent of character attack this effect inherit
    public float knockback; // knock back amount
    public int poiseBreak; // poise break amount
    public float energyDelta; // energy gain/consume amount
    public float lifetime; // effect lifetime after instantiate, normally is windup + duration
    public Vector2 velocity; // projectile velocity
    public Elements infusedElement; // the element type current effect is
    public bool destroyOnHit; // whether projectile is destroyed on hit

    // take a snapshot of attacker's current stats
    public float SSAttack;
    public float SSCR;
    public float SSCD;
    public float SSMaxHealth;
    public float SSHealth;

    public GameObject attacker;
    private EntityController attackerEC;

    Material effectMaterial;

    void Awake()
    {
        attackerEC = attacker.GetComponent<EntityController>();
        effectMaterial = GetComponent<SpriteRenderer>().material;
        switch (infusedElement)
        {
            case (Elements.physical):
                effectMaterial.SetColor("_Color_A", new Color(3, 3, 3));
                effectMaterial.SetColor("_Color_B", new Color(0, 0, 0));
                break;
            case (Elements.fire):
                effectMaterial.SetColor("_Color_A", new Color(3, 0, 0));
                effectMaterial.SetColor("_Color_B", new Color(3, 2.4f, 0));
                break;
            case (Elements.ice):
                effectMaterial.SetColor("_Color_A", new Color(0, 1.5f, 3));
                effectMaterial.SetColor("_Color_B", new Color(3, 3, 3));
                break;
            case (Elements.electric):
                effectMaterial.SetColor("_Color_A", new Color(1.5f, 0, 3));
                effectMaterial.SetColor("_Color_B", new Color(3, 1.5f, 0));
                //effectMaterial.SetColor("_Color_B", new Color(1.5f, 0, 1.5f));
                break;
            case (Elements.light):
                effectMaterial.SetColor("_Color_A", new Color(3, 1.5f, 0));
                effectMaterial.SetColor("_Color_B", new Color(3, 3, 3));
                break;
            case (Elements.dark):
                effectMaterial.SetColor("_Color_A", new Color(0, 0, 0.15f));
                effectMaterial.SetColor("_Color_B", new Color(0, 0, 1));
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(gameObject.name + " Lifetime: " + lifetime);
        velocity = attacker.transform.rotation * velocity;
        transform.rotation = attacker.transform.rotation;
    }

    void OnEnable()
    {
        if (energyDelta < 0)
            attackerEC.GainEnergy(energyDelta);

        foreach (ItemEffect ie in attachedFollowUpEffects)
        {
            if (ie.followUpTrigger == FollowUpTrigger.EffectActive)
                OnEffectActive?.Invoke(this, ie, attacker);
        }
    }

    void OnDisable()
    {
        foreach (ItemEffect ie in attachedFollowUpEffects)
        {
            if (ie.followUpTrigger == FollowUpTrigger.EffectEnd)
                OnEffectActive?.Invoke(this, ie, attacker);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)velocity;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
    }

    public void startGhost() => attackerEC.ghost = true;

    public void endGhost() => attackerEC.ghost = false;

    public void startInvincible() => attackerEC.invincible = true;

    public void endInvincible() => attackerEC.invincible = false;

    public void startControlImmune() => attackerEC.controlImmune = true;

    public void endControlImmune() => attackerEC.controlImmune = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        Debug.Log(collision.tag);

        if (attacker.CompareTag("Player") && collision.CompareTag("Enemy")
            || attacker.CompareTag("Enemy") && collision.CompareTag("Player"))
        {
            EntityController ec = collision.GetComponent<EntityController>();
            float finalDamage = attackPercent / 100f * SSAttack;
            bool critted = Random.Range(0f, 1f) <= SSCR;
            if (critted)
            {
                finalDamage *= 1 + SSCD;
            }
            Debug.Log(infusedElement);
            bool killed = collision.GetComponent<EntityController>()
                .TakeDamage((int)finalDamage, poiseBreak, critted, infusedElement);
            attackerEC.TriggerStatusEffect(StatusEffectObject.TriggerType.OnHit, collision.gameObject);
            if (energyDelta > 0)
                attackerEC.GainEnergy(energyDelta);
            Debug.Log("Trigger Hit, going through followup");
            foreach (ItemEffect ie in attachedFollowUpEffects)
            {
                Debug.Log(ie.followUpTrigger);
                if (ie.followUpTrigger == FollowUpTrigger.EffectHit)
                {
                    Debug.Log("Invoke effect");
                    OnEffectHit?.Invoke(this, ie, attacker);
                }
            }
            if (killed) attackerEC.TriggerStatusEffect(StatusEffectObject.TriggerType.OnKill, collision.gameObject);
            if (energyDelta > 0)
            {
                attackerEC.GainEnergy(energyDelta);
            }
            if (knockback > 0)
            {
                collision.GetComponent<EntityController>().KnockBack(
                    (collision.transform.position - transform.position).normalized * knockback,
                    0.2f);
            }
            if (destroyOnHit) Destroy(gameObject);
        }

        if (collision.tag == "Attack"
            && collision.GetComponent<EffectManager>().attacker.tag == "Enemy")
        {
            foreach (ItemEffect ie in attachedFollowUpEffects)
            {
                if (ie.followUpTrigger == FollowUpTrigger.EffectBlock)
                {
                    GameManager.QuickTimeScale(0.3f, 0.3f);
                    OnEffectHit?.Invoke(this, ie, attacker);
                    attackerEC.EnterCombat();
                }
            }
        }
    }
}
