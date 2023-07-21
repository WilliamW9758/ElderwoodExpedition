using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum EnemyClass
{
    Marksman,
    Mage,
    Assassin,
    Warrior,
    Tank
}

public class EnemyController : EntityController
{
    public EnemyClass enemyClass;

    public enum EnemyStateMachine
    {
        Patrol,
        Alert,
        Preperation,
        Attack,
        Retreat,
    }
    public EnemyStateMachine state;

    public int stunAmount;
    public int stunCount;

    public float attackRadius;
    public float aggroRadius;

    public ItemObject testAttack;
    public ItemObject testAttack1;
    public ItemObject testAttack2;
    public ItemObject testAttack3;
    public ItemObject testAttack4;

    private Vector2 randomVec;
    private Vector2 originalPos;

    public GameObject player;
    public Vector2 relativePos;

    public GameObject alertAnim;
    public GameObject deathAnim;

    public Image breakFill;

    protected new void Start()
    {
        base.Start();
        StartCoroutine(RandomMovement());
        player = GameObject.FindGameObjectWithTag("Player");
        stunCount = stunAmount;
        originalPos = transform.position;

        wc.weaponLeft.AddItem(new Item(testAttack));
        wc.weaponLeft.AddItem(new Item(testAttack1));
        wc.weaponLeft.AddItem(new Item(testAttack2));
        wc.weaponRight.AddItem(new Item(testAttack3));
        wc.weaponRight.AddItem(new Item(testAttack4));
    }

    protected new void Update()
    {
        relativePos = player.transform.position - transform.position;
        base.Update();

        BehaviorLogic();
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected void BehaviorLogic()
    {
        if (state == EnemyStateMachine.Patrol)
        {
            moveVec = (randomVec + randomVec.magnitude * (originalPos - (Vector2)transform.position) * 0.3f).normalized * 0.4f;
            if (relativePos.magnitude <= aggroRadius)
            {
                StartCoroutine("StartAlert");
                state = EnemyStateMachine.Alert;
            }
        }
        else if (state == EnemyStateMachine.Alert)
        {
            moveVec = Vector2.zero;
            if (relativePos.magnitude > aggroRadius)
            {
                StopCoroutine("StartAlert");
                Debug.Log("Should stop alert");
                state = EnemyStateMachine.Patrol;
            }
        }
        else if (state == EnemyStateMachine.Preperation)
        {
            PreperationLogic();
        }
        else if (state == EnemyStateMachine.Attack)
        {
            AttackLogic(relativePos);
        }
        else if (state == EnemyStateMachine.Retreat)
        {
            moveVec = (-relativePos.normalized + randomVec).normalized;
            if ((wc.nextItemLeft.Length > 0 && wc.nextItemRight.Length > 0)
                || (player.GetComponent<EntityController>().health < health
                && (wc.nextItemLeft.Length > 0 || wc.nextItemRight.Length > 0)))
            {
                state = EnemyStateMachine.Attack;
            }
        }
    }

    protected override Quaternion calcTargetRot()
    {
        if (state == EnemyStateMachine.Alert || state == EnemyStateMachine.Preperation)
        {
            return Quaternion.LookRotation(Vector3.forward, relativePos);
        }
        else
        {
            return base.calcTargetRot();
        }
    }

    private IEnumerator StartAlert()
    {
        yield return new WaitForSeconds(1f);
        state = EnemyStateMachine.Preperation;
    }

    protected void PreperationLogic()
    {
        // place holder
        if (relativePos.magnitude > 2 * attackRadius)
        {
            float tempX = (relativePos.magnitude - 2 * attackRadius);
            moveVec = ((1f / (1 + Mathf.Exp(-tempX)) * 2 - 1)
                * relativePos.normalized
                + randomVec).normalized;
        } else if (relativePos.magnitude < attackRadius)
        {
            float tempX = (relativePos.magnitude - attackRadius);
            moveVec = ((1f / (1 + Mathf.Exp(-tempX)) * 2 - 1)
                * relativePos.normalized
                + randomVec).normalized;
        } else
        {
            moveVec = randomVec;
        }

        if (!(wc.nextItemLeft.Length > 0) && !(wc.nextItemRight.Length > 0)
            && health < maxHealth * 0.2f)
        {
            state = EnemyStateMachine.Retreat;
        }

        if (wc.nextItemLeft.Length > 0 || wc.nextItemRight.Length > 0)
        {
            state = EnemyStateMachine.Attack;
        }
    }

    protected virtual void AttackLogic(Vector2 relativePos)
    {
        if (relativePos.magnitude <= attackRadius
            && wc.canAttack && !GameManager.IsGamePaused)
        {
            if (wc.nextItemLeft.Length > 0 && wc.AdvanceWeaponLeft())
            {
                wc.UpdateLeft();
                wc.TriggerCurrentItem();
            }
            else if (wc.nextItemRight.Length > 0 && wc.AdvanceWeaponRight())
            {
                wc.UpdateRight();
                wc.TriggerCurrentItem();
            }
            else
            {
                state = EnemyStateMachine.Preperation;
            }
        } else
        {
            moveVec = relativePos.normalized;
        }

    }

    protected IEnumerator RandomMovement()
    {
        while (true)
        {
            randomVec = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            randomVec = Vector2.zero;
            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    public override bool TakeDamage(int damage, int breakAmount, bool _crit, Elements type = Elements.physical)
    {
        if (state == EnemyStateMachine.Patrol || state == EnemyStateMachine.Alert)
        {
            state = EnemyStateMachine.Preperation;
        }
        bool killed = base.TakeDamage(damage, breakAmount, _crit, type);

        if (stunCount > 0)
        {
            stunCount -= breakAmount;
            if (stunCount <= 0)
            {
                stunCount = 0;
                StartCoroutine(Stun_Cor(1f));
            }
        }
        //breakFill.fillAmount = (float)stunCount / stunAmount;
        return killed;
    }

    protected new IEnumerator Stun_Cor(float seconds)
    {
        stun = true;
        yield return new WaitForSeconds(seconds);
        stun = false;
        stunCount = stunAmount;
        //breakFill.fillAmount = stunCount / stunAmount;
    }

    protected override void OnDeath()
    {
        //Destroy(gameObject);
        health = maxHealth;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
