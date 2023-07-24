using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

public enum Elements
{
    physical,
    fire,
    ice,
    electric,
    light,
    dark
}

public class EntityController : MonoBehaviour
{
    // stats and effects
    public float speed;
    public float speedMod;
    public float rotLerpSpeed;
    public int maxHealth;
    public int health;
    public float maxEnergy;
    public float energy;
    public float energyRegen;
    public float attack;
    public int attackFlatMod;
    public float attackRatioMod;
    public float critRate;
    public float critDamage;

    public bool controlLock;
    public bool invincible;
    public bool ghost;
    public bool controlImmune;
    public bool stun;
    public bool airborne;
    public bool dizzy;

    public Vector2 moveVec;
    protected Rigidbody2D rb;
    public Vector2 dashVelocity;
    public Vector2 pushVelocity;

    public Transform canvas;
    public Image healthFill;
    public GameObject floatingTextPrefab;
    public GameObject floatingTextCritPrefab;

    public GameObject buffSlotPrefab;
    public GameObject buffDisplay;
    public Dictionary<StatusEffectManager, GameObject> buffSlots = new Dictionary<StatusEffectManager, GameObject>();
    public Sprite NoIcon;

    public WeaponController wc;

    public readonly Dictionary<StatusEffectObject, StatusEffectManager> buffs = new Dictionary<StatusEffectObject, StatusEffectManager>();

    public Elements currentElement;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wc = GetComponent<WeaponController>();
        //canvas = GameObject.FindGameObjectWithTag("MainCanvas").transform;
        currentElement = Elements.physical;
        health = maxHealth;
        energy = maxEnergy;
    }

    protected virtual void Update()
    {
        if (ghost)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            GetComponent<Collider2D>().enabled = true;
        }

        if (!GameManager.IsGamePaused)
        {
            foreach (var buff in buffs.Values.ToList())
            {
                buff.Tick(Time.deltaTime);
                UpdateBuffDisplayStats(buff);
                BuffCheckFinish(buff);
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        GainEnergy(energyRegen * Time.fixedDeltaTime);

        rb.freezeRotation = controlLock;
        rb.angularVelocity = 0f;

        if (controlLock)
        {
            rb.velocity = dashVelocity + pushVelocity;
        } else
        {
            rb.velocity = moveVec * speed * (1 + speedMod) + dashVelocity + pushVelocity;
            rb.rotation = Quaternion.Slerp(transform.rotation, calcTargetRot(), rotLerpSpeed * Time.deltaTime).eulerAngles.z;
        }
    }

    protected virtual Quaternion calcTargetRot()
    {
        if (moveVec.magnitude == 0)
        {
            return transform.rotation;
        } else
        {
            return Quaternion.LookRotation(Vector3.forward, moveVec);
        }
    }

    /**
     * Called when getting hit, return true if killed, false otherwise
     */
    public virtual bool TakeDamage(int damage, int breakAmount, bool _crit, Elements type = Elements.physical)
    {
        TriggerStatusEffect(StatusEffectObject.TriggerType.OnHit, gameObject);
        if (invincible)
        {
            ShowFloatingText("Immune"); // show immune
            return false;
        }
        Debug.Log(gameObject.name + " taken " + type + " damage with id " + (int)type);
        switch (type)
        {
            case (Elements.physical):
                ShowFloatingText(damage.ToString(), Color.white, crit : _crit);
                break;
            case (Elements.fire):
                Debug.Log("Showing Fire text");
                ShowFloatingText(damage.ToString(), new Color(1, 0.15f, 0.15f), crit: _crit);
                break;
            case (Elements.ice):
                ShowFloatingText(damage.ToString(), new Color(0.5f, 0.94f, 1), crit: _crit);
                break;
            case (Elements.electric):
                ShowFloatingText(damage.ToString(), new Color(0.84f, 0.17f, 0.9f), crit: _crit);
                break;
            case (Elements.light):
                ShowFloatingText(damage.ToString(), new Color(1, 0.9f, 0.4f), crit: _crit);
                break;
            case (Elements.dark):
                ShowFloatingText(damage.ToString(), new Color(0.2f, 0.17f, 0.9f), crit : _crit);
                break;
        }
        //animator.SetTrigger("getHit");
        StartCoroutine(TakeDamage_Cor());
        StartCoroutine(Dizzy_Cor(0.1f));
        //StartCoroutine(CameraController.cameraShake(0.05f, damage * 0.02f));
        health -= damage;
        TriggerStatusEffect(StatusEffectObject.TriggerType.OnTakeDamage);
        //healthFill.fillAmount = (float)health / maxHealth;
        if (health < 0)
        {
            OnDeath();
            return true;
        }
        return false;
    }

    protected virtual void OnDeath()
    {
        TriggerStatusEffect(StatusEffectObject.TriggerType.OnDeath, gameObject);
        // for testing
        health = maxHealth;
    }

    public virtual void StartDash(Vector2 dir)
    {
        dashVelocity = dir;
    }

    public void EndDash()
    {
        dashVelocity = Vector2.zero;
    }

    public virtual void KnockBack(Vector2 knockVec, float seconds)
    {
        IEnumerator KnockBack_Cor(Vector2 knockVec, float seconds)
        {
            pushVelocity = knockVec;
            yield return new WaitForSeconds(seconds);
            pushVelocity = Vector2.zero;
        }
        StartCoroutine(KnockBack_Cor(knockVec, seconds));
    }

    public virtual void GainEnergy(float amount)
    {
        energy = energy + amount > maxEnergy ? maxEnergy : energy + amount;
    }

    protected IEnumerator TakeDamage_Cor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.material.SetInt("_hit", 1);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 0);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 1);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 0);
    }

    protected IEnumerator Stun_Cor(float seconds)
    {
        stun = true;
        yield return new WaitForSeconds(seconds);
        stun = false;
    }

    protected IEnumerator Dizzy_Cor(float seconds)
    {
        dizzy = true;
        yield return new WaitForSeconds(seconds);
        dizzy = false;
    }

    public void ShowFloatingText(string content, Color color = default, bool onEntity = true, bool crit = false)
    {
        if (color == default) color = Color.white;

        if (onEntity)
        {
            GameObject message;
            if (!crit)
                message = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
            else
                message = Instantiate(floatingTextCritPrefab, transform.position, Quaternion.identity);
            message.GetComponent<TMP_Text>().color = color;
            message.GetComponent<TMP_Text>().text = content;
        } else
        {
            UIManager.SpawnSystemText?.Invoke(content, color);
        }

    }

    public void AddStatusEffect(StatusEffectManager buff)
    {
        if (buff == null)
        {
            //throw new System.NullReferenceException("Buff added is null");
            Debug.Log("Applied no buff");
            return;
        }
        if (buffs.ContainsKey(buff.buff))
        {
            buffs[buff.buff].Activate();
        }
        else
        {
            buffs.Add(buff.buff, buff);
            if (buffDisplay != null)
            {
                GameObject buffIcon = Instantiate(buffSlotPrefab, buffDisplay.transform);
                Sprite icon = buff.buff.icon != null ? buff.buff.icon : NoIcon;
                buffIcon.transform.Find("BuffImage").GetComponent<Image>().sprite = icon;
                buffSlots.Add(buff, buffIcon);
            }
            buff.Activate();
        }
    }

    public void TriggerStatusEffect(StatusEffectObject.TriggerType triggerType, GameObject target = null)
    {
        foreach (var buff in buffs.Values.ToList())
        {
            buff.Trigger(triggerType, target);
            BuffCheckFinish(buff);
        }
    }

    protected void BuffCheckFinish(StatusEffectManager buff)
    {
        if (buff.IsFinished)
        {
            RemoveBuff(buff.buff);
        }
    }

    public void RemoveBuff(StatusEffectObject buff)
    {
        StatusEffectManager bm = buffs[buff];
        if (buffDisplay != null)
        {
            Destroy(buffSlots[bm]);
            buffSlots.Remove(bm);
        }
        buffs.Remove(buff);
        
    }

    protected void UpdateBuffDisplayStats(StatusEffectManager buff)
    {
        if (buffDisplay != null)
        {
            if (buff.buff.isTimeBased)
            {
                if (buff.maxDuration > 1000)
                {
                    buffSlots[buff].transform.Find("BuffFill").GetComponent<Image>().fillAmount = 0;
                } else
                {
                    buffSlots[buff].transform.Find("BuffFill").GetComponent<Image>().fillAmount =
                    1 - buff.durationSeconds / buff.maxDuration;
                }
                buffSlots[buff].transform.Find("BuffStack").GetComponent<TMP_Text>().text =
                    buff.effectStacks.ToString();
            }
            else
            {
                buffSlots[buff].transform.Find("BuffFill").GetComponent<Image>().fillAmount = 0;
                if (buff.durationStacks == 0)
                {
                    buffSlots[buff].transform.Find("BuffStack").GetComponent<TMP_Text>().text = "";
                } else
                {
                    buffSlots[buff].transform.Find("BuffStack").GetComponent<TMP_Text>().text =
                        buff.durationStacks.ToString();
                }
            }
        }
    }
}