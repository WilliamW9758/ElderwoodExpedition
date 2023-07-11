using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyAlertManager : MonoBehaviour
{
    public GameObject origin;

    private void Awake()
    {
        origin = transform.parent.gameObject;
    }

    public void alertEnd()
    {
        origin.GetComponent<BasicEnemyController>().currentState = BasicEnemyController.EnemyStateMachine.chase;
        Destroy(gameObject);
    }
}
