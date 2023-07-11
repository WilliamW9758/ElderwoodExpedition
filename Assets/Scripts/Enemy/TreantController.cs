using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreantController : BasicEnemyController
{
    public GameObject basicAttack;
    public float attackCD;
    public float attackTimer;
    public Animator anim;

    public GameObject currentAttack;

    new void Start()
    {
        base.Start();
        currentState = EnemyStateMachine.idle;
        anim = GetComponent<Animator>();
    }

    new void Update()
    {
        base.Update();

        Vector2 toPlayer = player.transform.position - transform.position;
        attackTimer = attackTimer > 0 ? attackTimer - Time.deltaTime : 0;
        anim.SetBool("Stun", stun || dizzy);

        // first enter stun
        if (stun == true && currentState != EnemyStateMachine.stun)
        {
            currentState = EnemyStateMachine.stun;
            //attackTimer = attackCD;
            if (currentAttack != null) Destroy(currentAttack);
            rb.mass = 10;
        }

        // first enter dizzy
        if (dizzy && (currentState != EnemyStateMachine.stun && currentState != EnemyStateMachine.dizzy))
        {
            currentState = EnemyStateMachine.dizzy;
            rb.mass = 10;
        }
        // state machine
        if (currentState == EnemyStateMachine.idle)
        {
            moveVec = Vector2.zero;
            if (toPlayer.magnitude < aggroRadius)
            {
                currentState = EnemyStateMachine.alert;
                Instantiate(alertAnim, transform.position, Quaternion.identity, transform);
            }
        }
        else if (currentState == EnemyStateMachine.alert)
        {
            // wait for anim end
        }
        else if (currentState == EnemyStateMachine.chase)
        {
            moveVec = toPlayer.normalized;
            if ((player.transform.position - transform.position).magnitude <
                basicAttack.GetComponent<CapsuleCollider2D>().size.y)
            {
                currentState = EnemyStateMachine.attack;
                attackTimer = attackCD;
                currentAttack = Instantiate(basicAttack, transform.position,
                    Quaternion.LookRotation(Vector3.forward,
                    player.transform.position - transform.position), transform);
            }
        }
        else if (currentState == EnemyStateMachine.attack)
        {
            // wait for anim end
        }
        else if (currentState == EnemyStateMachine.dodge)
        {
            if (attackTimer == 0)
            {
                currentState = EnemyStateMachine.chase;
            }
            if (toPlayer.magnitude <= 1.5 * basicAttack.GetComponent<CapsuleCollider2D>().size.y)
            {
                moveVec = -toPlayer.normalized;
            } else
            {
                moveVec = Vector3.Cross(player.transform.position - transform.position,
                    Vector3.forward).normalized;
            }
        }
        else if (currentState == EnemyStateMachine.death)
        {
            Destroy(gameObject);
            //Instantiate(deathAnim, transform);
        }
        else if (currentState == EnemyStateMachine.stun)
        {
            // play stun anim
            if (!stun)
            {
                if (attackTimer > 0)
                {
                    currentState = EnemyStateMachine.dodge;
                } else
                {
                    currentState = EnemyStateMachine.chase;
                }
                rb.mass = 1;
            }
        }
        else if (currentState == EnemyStateMachine.dizzy)
        {
            // play stun anim
            if (!dizzy)
            {
                if (attackTimer > 0)
                {
                    currentState = EnemyStateMachine.dodge;
                }
                else
                {
                    currentState = EnemyStateMachine.chase;
                }
                rb.mass = 1;
            }
        }
    }

    new void FixedUpdate()
    {
        if (currentState == EnemyStateMachine.attack ||
            currentState == EnemyStateMachine.stun ||
            currentState == EnemyStateMachine.dizzy) {
            moveVec = Vector2.zero;
        }
        base.FixedUpdate();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, basicAttack.GetComponent<CapsuleCollider2D>().size.y);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 1.5f * basicAttack.GetComponent<CapsuleCollider2D>().size.y);
    }
}
