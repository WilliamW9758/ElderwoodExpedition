using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyController : EntityController
{
    public int stunAmount;
    public int stunCount;

    public float aggroRadius;

    public GameObject player;

    public GameObject alertAnim;
    public GameObject deathAnim;

    public Image breakFill;

    protected new void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player");
        stunCount = stunAmount;
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override bool TakeDamage(int damage, int breakAmount, Elements type = Elements.physical)
    {
        bool killed = base.TakeDamage(damage, breakAmount);

        if (stunCount > 0)
        {
            stunCount -= breakAmount;
            if (stunCount <= 0)
            {
                stunCount = 0;
                StartCoroutine(Stun_Cor(1f));
            }
        }
        breakFill.fillAmount = (float)stunCount / stunAmount;
        return killed;
    }

    protected new IEnumerator Stun_Cor(float seconds)
    {
        stun = true;
        yield return new WaitForSeconds(seconds);
        stun = false;
        stunCount = stunAmount;
        breakFill.fillAmount = stunCount / stunAmount;
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}
