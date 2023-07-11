using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCounterAttackManager : AttackManager
{
    public bool counter;
    public bool counterEnbable;
    public Animator anim;

    private void Start()
    {
        counter = false;
        counterEnbable = false;
        anim = GetComponent<Animator>();
    }

    public new void startAttack()
    {
        Debug.Log("Switch to attack");
        pc.controlLock = true;
        counterEnbable = true;
        wm.attackState = WeaponManager.AttackState.attack;
    }

    public new void startPostAttack()
    {
        base.startPostAttack();
        if (counter)
        {
            wm.currentPos = WeaponManager.SwordPosition.right;
        }
    }

    private new void OnTriggerEnter2D(Collider2D collision)
    {
        if (!counter && (collision.tag == "Attack" && collision.transform.parent.tag == "Enemy"))
        {
            if (counterEnbable)
            {
                counter = true;
                anim.SetBool("counter", true);
                StartCoroutine(EntityController.QuickTimeScale(0.3f, 0.2f));
                wm.swordAnim.SetInteger("swordState", 2);
            } else
            {
                anim.SetBool("blocked", true);
            }
        } else if (counter && (collision.tag == "Hurtbox" && collision.transform.parent.tag == "Enemy"))
        {
            base.OnTriggerEnter2D(collision);
        }
    }

    public void noCounter()
    {
        counterEnbable = false;
    }
}
