using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public Spell thisSpell;
    public GameObject player;
    public PlayerController pc;
    public WeaponManager wm;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
        wm = player.GetComponent<WeaponManager>();
    }

    public void startPreCast()
    {
        //pc.controlLock = true;
        pc.TriggerBuff(Buff.TriggerType.anyCast);
        pc.TriggerBuff(Buff.TriggerType.spellCast);
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        wm.attackState = WeaponManager.AttackState.preAttack;
    }

    public void startCast()
    {
        //pc.controlLock = true;
        Debug.Log(thisSpell.buffsApplied[0].name);
        foreach (Buff buff in thisSpell.buffsApplied)
        {
            pc.AddBuff(buff.InitializeBuff(player));
        }
        wm.attackState = WeaponManager.AttackState.attack;
    }

    public void startPostCast()
    {
        //pc.controlLock = true;
        wm.attackState = WeaponManager.AttackState.postAttack;
    }

    public void endCast()
    {
        //pc.controlLock = false;
        pc.TriggerBuff(Buff.TriggerType.spellEnd);
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
}
