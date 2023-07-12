using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelEndController : MonoBehaviour
{
    public GameManager gm;

    public GameObject CombatDoor;
    public GameObject EventDoor;
    public GameObject RestDoor;
    public GameObject EliteDoor;
    public GameObject BossDoor;


    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        GameManager.LevelComplete += OpenDoors;
    }

    private void OnDisable()
    {
        GameManager.LevelComplete -= OpenDoors;
    }

    public void OpenDoors()
    {
        List<Vector3> locs = new List<Vector3>();
        if (GameManager.levelPresets[GameManager.currentLevel + 1].Count >= 2)
        {
            List<LevelType> temp = new List<LevelType>(GameManager.levelPresets[GameManager.currentLevel + 1]);
            temp.OrderBy(x => Guid.NewGuid()).ToList().Take(2);
            SpawnDoor(temp[0], new Vector3(-3, 0, 0));
            SpawnDoor(temp[1], new Vector3(3, 0, 0));
        } else
        {
            SpawnDoor(GameManager.levelPresets[GameManager.currentLevel + 1][0], Vector3.zero);
        }
    }

    private void SpawnDoor(LevelType lvl, Vector3 loc)
    {
        switch (lvl)
        {
            case (LevelType.Combat):
                Instantiate(CombatDoor, transform.position + loc, Quaternion.identity,
                    transform);
                break;
            case (LevelType.Event):
                Instantiate(EventDoor, transform.position + loc, Quaternion.identity,
                    transform);
                break;
            case (LevelType.Rest):
                Instantiate(RestDoor, transform.position + loc, Quaternion.identity,
                    transform);
                break;
            case (LevelType.Elite):
                Instantiate(EliteDoor, transform.position + loc, Quaternion.identity,
                    transform);
                break;
            case (LevelType.Boss):
                Instantiate(BossDoor, transform.position + loc, Quaternion.identity,
                    transform);
                break;
        }
    }
}
