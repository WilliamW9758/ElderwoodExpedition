using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class EntityController : MonoBehaviour
{
    // stats and effects
    public float speed;
    public float speedMod;
    public int maxHealth;
    public int health;
    public float critRate;
    public float critDamage;
    public int damageFlatMod;
    public float damageRatioMod;
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
    public GameObject systemTextPrefab;

    public GameObject buffSlotPrefab;
    public GameObject buffDisplay;
    public Dictionary<BuffManager, GameObject> buffSlots = new Dictionary<BuffManager, GameObject>();
    public Sprite NoIcon;

    public readonly Dictionary<Buff, BuffManager> buffs = new Dictionary<Buff, BuffManager>();

    public enum Elements
    {
        physical,
        fire,
        ice,
        electric,
        light,
        dark
    }
    public Elements currentElement;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        canvas = GameObject.FindGameObjectWithTag("MainCanvas").transform;
        currentElement = Elements.physical;
        health = maxHealth;
    }

    protected void Update()
    {
        if (ghost)
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            GetComponent<BoxCollider2D>().enabled = true;
        }

        if (Time.timeScale != 0)
        {
            foreach (var buff in buffs.Values.ToList())
            {
                buff.Tick(Time.deltaTime);
                UpdateBuffDisplayStats(buff);
                BuffCheckFinish(buff);
            }
        }
    }

    protected void FixedUpdate()
    {
        rb.velocity = moveVec * speed * (1 + speedMod) + dashVelocity + pushVelocity;
    }

    /**
     * Called when getting hit, return true if killed, false otherwise
     */
    public virtual bool TakeDamage(int damage, int breakAmount, Elements type = Elements.physical)
    {
        TriggerBuff(Buff.TriggerType.getHit);
        if (invincible)
        {
            ShowFloatingText("Immune"); // show immune
            return false;
        }
        Debug.Log(gameObject.name + " taken " + type + " damage with id " + (int)type);
        switch (type)
        {
            case (Elements.physical):
                ShowFloatingText(damage.ToString(), Color.white);
                break;
            case (Elements.fire):
                ShowFloatingText(damage.ToString(), new Color(1, 0.15f, 0.15f));
                break;
            case (Elements.ice):
                ShowFloatingText(damage.ToString(), new Color(0.5f, 0.94f, 1));
                break;
            case (Elements.electric):
                ShowFloatingText(damage.ToString(), new Color(0.84f, 0.17f, 0.9f));
                break;
            case (Elements.light):
                ShowFloatingText(damage.ToString(), new Color(1, 0.9f, 0.4f));
                break;
            case (Elements.dark):
                ShowFloatingText(damage.ToString(), new Color(0.2f, 0.17f, 0.9f));
                break;
        }
        //animator.SetTrigger("getHit");
        StartCoroutine(TakeDamage_Cor());
        StartCoroutine(Dizzy_Cor(0.1f));
        //StartCoroutine(CameraController.cameraShake(0.05f, damage * 0.02f));
        health -= damage;
        TriggerBuff(Buff.TriggerType.takeDamage);
        healthFill.fillAmount = (float)health / maxHealth;
        if (health < 0)
        {
            OnDeath();
            return true;
        }
        return false;
    }

    protected void OnDeath()
    {
        // for testing
        health = maxHealth;
    }

    public void StartDash(Vector2 dir)
    {
        dashVelocity = dir;
    }

    public void EndDash()
    {
        dashVelocity = Vector2.zero;
    }

    public void KnockBack(Vector2 knockVec, float seconds)
    {
        IEnumerator KnockBack_Cor(Vector2 knockVec, float seconds)
        {
            pushVelocity = knockVec;
            yield return new WaitForSeconds(seconds);
            pushVelocity = Vector2.zero;
        }
        StartCoroutine(KnockBack_Cor(knockVec, seconds));
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

    public void ShowFloatingText(string content, Color color = default, bool onEntity = true)
    {
        if (color == default) color = Color.white;

        if (onEntity)
        {
            GameObject message = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
            message.GetComponent<TMP_Text>().color = color;
            message.GetComponent<TMP_Text>().text = content;
        } else
        {
            //Debug.Log(canvas);
            //Debug.Log(systemTextPrefab);
            GameObject message = Instantiate(systemTextPrefab, canvas);
            //Debug.Log(message);
            //Debug.Log(message.GetComponentInChildren<TMP_Text>());
            message.GetComponentInChildren<TMP_Text>().color = color;
            message.GetComponentInChildren<TMP_Text>().text = content;
        }

    }

    public static IEnumerator QuickTimeScale(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public virtual void AddBuff(BuffManager buff)
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

    public void TriggerBuff(Buff.TriggerType triggerType, GameObject target = null)
    {
        foreach (var buff in buffs.Values.ToList())
        {
            buff.Trigger(triggerType, target);
            BuffCheckFinish(buff);
        }
    }

    private void BuffCheckFinish(BuffManager buff)
    {
        if (buff.IsFinished)
        {
            buffs.Remove(buff.buff);
            if (buffDisplay != null)
            {
                Destroy(buffSlots[buff]);
                buffSlots.Remove(buff);
            }
        }
    }

    private void UpdateBuffDisplayStats(BuffManager buff)
    {
        if (buffDisplay != null)
        {
            if (buff.buff.isTimeBased)
            {
                buffSlots[buff].transform.Find("BuffFill").GetComponent<Image>().fillAmount =
                    1 - buff.durationSeconds / buff.maxDuration;
                buffSlots[buff].transform.Find("BuffStack").GetComponent<TMP_Text>().text =
                    buff.effectStacks.ToString();
            }
            else
            {
                buffSlots[buff].transform.Find("BuffFill").GetComponent<Image>().fillAmount = 0;
                buffSlots[buff].transform.Find("BuffStack").GetComponent<TMP_Text>().text =
                    buff.durationStacks.ToString();
            }
        }
    }
}