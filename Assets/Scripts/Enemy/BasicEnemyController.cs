using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : EnemyController
{
    public enum EnemyStateMachine
    {
        idle,
        alert,
        chase,
        attack,
        dodge,
        death,
        stun,
        dizzy
    }

    public EnemyStateMachine currentState;
}
