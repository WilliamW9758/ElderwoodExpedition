using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyAttackManager : MonoBehaviour
{
    public GameObject origin;
    public Attack thisAttack;

    private void Awake()
    {
        origin = transform.parent.gameObject;
    }

    public void startAttack()
    {
        origin.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        disableCollider();
    }

    public void finishAttack()
    {
        origin.GetComponent<BasicEnemyController>().currentState = BasicEnemyController.EnemyStateMachine.dodge;
        Destroy(gameObject);
    }

    public void enableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
    }

    public void disableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Enter: " + collision.name);
        //Debug.Log(collision.transform.parent.tag);
        if (collision.tag == "Hurtbox" && collision.transform.parent.tag == "Player")
        {
            collision.transform.parent.GetComponent<PlayerController>().TakeDamage(thisAttack.damage);
            disableCollider();
            if (thisAttack.knockback > 0)
            {
                collision.transform.parent.GetComponent<PlayerController>().KnockBack(
                    (collision.transform.position - transform.position).normalized * thisAttack.knockback,
                    0.1f);
            }
        }
    }
}
