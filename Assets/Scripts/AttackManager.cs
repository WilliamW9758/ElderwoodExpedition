using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public Attack thisAttack;
    public GameObject player;
    public PlayerController pc;
    public WeaponManager wm;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
        wm = player.GetComponent<WeaponManager>();
    }

    public void startPreAttack()
    {
        //Debug.Log("Switch to PreAttack");
        pc.controlLock = true;
        wm.currentPos = thisAttack.startPos;
        wm.swordAnim.SetInteger("swordState", thisAttack.swordStateID);
        pc.animator.SetInteger("playerState", thisAttack.playerStateID);
        pc.TriggerBuff(Buff.TriggerType.anyCast);
        pc.TriggerBuff(Buff.TriggerType.attackCast);
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        wm.attackState = WeaponManager.AttackState.preAttack;
        wm.StopAllCoroutines();
    }

    public void startAttack()
    {
        //Debug.Log("Switch to attack");
        pc.controlLock = true;
        pc.StartDash(transform.rotation * thisAttack.moveVec);
        wm.attackState = WeaponManager.AttackState.attack;
    }

    public void startPostAttack()
    {
        //Debug.Log("Switch to postAttack");
        pc.controlLock = true;
        pc.EndDash();
        wm.currentPos = thisAttack.endPos;
        Debug.Log("Currnet Pos: " + wm.currentPos);
        wm.attackState = WeaponManager.AttackState.postAttack;
    }

    public void finishAttack()
    {
        //Debug.Log("Switch to finish");
        //Debug.Log("left:" + WeaponManager.weaponPtrLeft);
        //Debug.Log("left Count:" + WeaponManager.weaponOrderLeft.Count);
        pc.controlLock = false;
        wm.StartSwordPosReset();
        wm.swordAnim.SetInteger("swordState", 0);
        pc.animator.SetInteger("playerState", 0);
        wm.swordRot = Quaternion.identity;
        pc.TriggerBuff(Buff.TriggerType.attackEnd);
        if (wm.weaponPtrLeft >= wm.weaponOrderLeft.Count
            && wm.weaponOrderLeft.Count > 0)
        {
            Debug.Log("Reload Left");
            wm.WeaponReset(true, false);
            wm.ReloadLeft();
        }
        if (wm.weaponPtrRight >= wm.weaponOrderRight.Count
            && wm.weaponOrderRight.Count > 0)
        {
            Debug.Log("Reload Right");
            wm.WeaponReset(false, true);
            wm.ReloadRight();
        }
        wm.attackState = WeaponManager.AttackState.canAttack;
        //Debug.Log(WeaponManager.attackState);
        Destroy(gameObject);
    }

    public void startGhost()
    {
        pc.ghost = true;
    }

    public void endGhost()
    {
        pc.ghost = false;
    }

    public void startInvincible()
    {
        pc.invincible = true;
    }

    public void endInvincible()
    {
        pc.invincible = false;
    }

    public void startControlImmune()
    {
        pc.controlImmune = true;
    }

    public void endControlImmune()
    {
        pc.controlImmune = false;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Hurtbox" && collision.transform.parent.tag == "Enemy")
        {
            Debug.Log(pc.currentElement);
            bool killed = collision.transform.parent.GetComponent<EnemyController>()
                .TakeDamage((int)((thisAttack.damage + pc.damageFlatMod) * (1 + pc.damageRatioMod)),
                thisAttack.stunAmount, pc.currentElement);
            Debug.Log(pc.currentElement);
            pc.TriggerBuff(Buff.TriggerType.attackHit, collision.transform.parent.gameObject);
            if (killed) pc.TriggerBuff(Buff.TriggerType.attackKill, collision.transform.parent.gameObject);
            if (thisAttack.energyDelta > 0)
            {
                pc.GainEnergy(thisAttack.energyDelta);
            }
            if (thisAttack.knockback > 0)
            {
                collision.transform.parent.GetComponent<EnemyController>().KnockBack(
                    (collision.transform.position - transform.position).normalized * thisAttack.knockback,
                    0.2f);
            }
        }
    }
}
