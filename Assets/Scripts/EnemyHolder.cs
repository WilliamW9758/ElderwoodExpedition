using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHolder : MonoBehaviour
{
    public List<EnemyController> Enemies;
    public float deaggroRange;
    public bool Aggro;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var enemy in Enemies)
        {
            enemy.originalPos = transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Aggro)
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.state != EnemyController.EnemyStateMachine.Patrol
                    && enemy.state != EnemyController.EnemyStateMachine.Alert)
                {
                    Aggro = true;
                    foreach (var e in Enemies)
                    {
                        e.Aggro(); 
                    }
                    break;
                }
                if (Aggro)
                    break;
            }
        } else
        {
            if ((GameManager.Instance.GetPlayerLocation - transform.position).magnitude > deaggroRange)
            {
                foreach (var enemy in Enemies)
                {
                    enemy.state = EnemyController.EnemyStateMachine.Patrol;
                }
                Aggro = false;
            }
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, deaggroRange);
    }
}
