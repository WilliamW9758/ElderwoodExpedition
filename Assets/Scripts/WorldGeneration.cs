using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

//[System.Serializable]
//[CreateAssetMenu(fileName = "New WorldGeneration", menuName = "Algorithm/New WorldGeneration")]
public class WorldGeneration: MonoBehaviour
{
    /**
     * Air = 0
     * Spawn = 1
     * Boss = 2
     * Elite = 3
     * Chest = 4
     * Fight = 5
     * Event = 6
     * Shop = 7
     * 
     * Generate logic:
     * First generate a 2D grid representing rooms,
     *      and a 2D grid that represent the edges
     * Place spawn at center
     * Place boss room at lease 3 blocks away
     * Place 2-3 elites
     * Place 1-3 chests
     * Randomly connects each generated room to another room
     *      using fight and event (70% fight, 30% event)
     * Set the edge along generated path as open
     * Iterate through each room and check if they are 
     *      connected to spawn using path finding
     * If not then connect room to spawn same as before
     * Check for special spawn conditions
     * Populate all the rooms with specific prefabs
     */

    //class Room<T> 
    //{
    //    public Room<T>? rightNode { get; set; }
    //    public Room<T>? leftNode { get; set; }
    //    public T value { get; set; }

    //    public Room(T value)
    //    {
    //        this.value = value;
    //        rightNode = null;
    //        leftNode = null;
    //    }

    //    public Room(T value, Room<T>? left, Room<T>? right) : this(value)
    //    {
    //        rightNode = right;
    //        leftNode = left;
    //    }

    //    public void SetLeftNode(Room<T> node)
    //    {
    //        leftNode = node;
    //    }

    //    public void SetRightNode(Room<T> node)
    //    {
    //        rightNode = node;
    //    }

    //    public Room<T>? GetRightNode() => rightNode;
    //    public Room<T>? GetLeftNode() => leftNode;

    //    public override string? ToString() => value?.ToString();
    //}

    public const int WORLD_LENGTH = 16;


    public Vector2 worldSize;
    public int[,] rooms;
    public Vector2 eliteMinMax;
    public Vector2 chestMinMax;
    public Vector2 shopMinMax;
    public int eliteCount;
    public int chestCount;
    public int fightCount;
    public int eventCount;
    public int shopCount;
    public List<Vector2> keyLocs;
    public float visualDelay;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Mouse0))
    //    {
    //        StartCoroutine(Generate());
            
    //    }
    //}

    //private void Start()
    //{
    //    if (worldSize.x < 5 || worldSize.y < 5)
    //    {
    //        throw new System.Exception("World too small");
    //    }
    //    Init();
    //    drawGizmos = true;
    //    StartCoroutine(Generate());
    //}

    //private IEnumerator Generate()
    //{
    //    if (worldSize.x < 5 || worldSize.y < 5)
    //    {
    //        throw new System.Exception("World too small");
    //    }
    //    // init
    //    Init();
    //    yield return new WaitForSecondsRealtime(visualDelay);

    //    // spawn
    //    Vector2 spawnLoc = new Vector2((int)worldSize.x / 2, (int)worldSize.y / 2);
    //    rooms[(int)spawnLoc.x, (int)spawnLoc.y] = 1;
    //    yield return new WaitForSecondsRealtime(visualDelay);

    //    //boss
    //    Vector2 bossLoc = new Vector2((int)worldSize.x / 2, (int)worldSize.y / 2);
    //    while (Vector2.Distance(bossLoc, spawnLoc) < 3)
    //    {
    //        bossLoc = new Vector2((int)Random.Range(0, worldSize.x), (int)Random.Range(0, worldSize.y));
    //    }
    //    rooms[(int)bossLoc.x, (int)bossLoc.y] = 2;
    //    keyLocs.Add(bossLoc);
    //    yield return new WaitForSecondsRealtime(visualDelay);

    //    // elite
    //    for (int i = 0; i < eliteCount; i++)
    //    {
    //        Vector2 eliteLoc = new Vector2((int)Random.Range(0, worldSize.x), (int)Random.Range(0, worldSize.y));
    //        while (rooms[(int)eliteLoc.x, (int)eliteLoc.y] != 0 || Vector2.Distance(eliteLoc, spawnLoc) < 2)
    //        {
    //            eliteLoc = new Vector2((int)Random.Range(0, worldSize.x), (int)Random.Range(0, worldSize.y));
    //        }
    //        rooms[(int)eliteLoc.x, (int)eliteLoc.y] = 3;
    //        keyLocs.Add(eliteLoc);
    //    }
    //    yield return new WaitForSecondsRealtime(visualDelay);

    //    // chest
    //    for (int i = 0; i < chestCount; i++)
    //    {
    //        Vector2 chestLoc = new Vector2((int)Random.Range(0, worldSize.x), (int)Random.Range(0, worldSize.y));
    //        while (rooms[(int)chestLoc.x, (int)chestLoc.y] != 0)
    //        {
    //            chestLoc = new Vector2((int)Random.Range(0, worldSize.x), (int)Random.Range(0, worldSize.y));
    //        }
    //        rooms[(int)chestLoc.x, (int)chestLoc.y] = 4;
    //        keyLocs.Add(chestLoc);
    //    }
    //    yield return new WaitForSecondsRealtime(visualDelay);

    //    // connect
    //    for (int i = 0; i < keyLocs.Count; i++)
    //    {
    //        int randIdx = Random.Range(0, keyLocs.Count);
    //        while (randIdx == i)
    //        {
    //            randIdx = Random.Range(0, keyLocs.Count);
    //        }
    //        ConnectTwoPoints(keyLocs[i], keyLocs[randIdx]);
    //        yield return new WaitForSecondsRealtime(visualDelay);
    //    }
    //}

    //private void Init()
    //{
    //    rooms = new int[(int)worldSize.x, (int)worldSize.y];
    //    eliteCount = (int)Random.Range(eliteMinMax.x, eliteMinMax.y + 1);
    //    chestCount = (int)Random.Range(chestMinMax.x, chestMinMax.y + 1);
    //    shopCount = (int)Random.Range(shopMinMax.x, shopMinMax.y + 1);
    //    keyLocs = new List<Vector2>();
    //}

    //private void OnDrawGizmos()
    //{
    //    if (!drawGizmos) return;
    //    for (int i = 0; i < worldSize.x; i++)
    //    {
    //        for (int j = 0; j < worldSize.y; j++)
    //        {
    //            switch (rooms[i,j])
    //            {
    //                case 0:
    //                    Gizmos.color = Color.white;
    //                    break;
    //                case 1:
    //                    Gizmos.color = Color.yellow;
    //                    break;
    //                case 2:
    //                    Gizmos.color = Color.red;
    //                    break;
    //                case 3:
    //                    Gizmos.color = Color.magenta;
    //                    break;
    //                case 4:
    //                    Gizmos.color = Color.grey;
    //                    break;
    //                case 5:
    //                    Gizmos.color = Color.blue;
    //                    break;
    //                case 6:
    //                    Gizmos.color = Color.cyan;
    //                    break;
    //            }
    //            Gizmos.DrawCube(new Vector3(i, j, 1), new Vector3(1, 1, 1));
    //        }
    //    }
    //}


    //private void ConnectTwoPoints(Vector2 first, Vector2 second)
    //{
    //    Vector2 curPoint = first;
    //    while (curPoint != second)
    //    {
    //        if (rooms[(int)curPoint.x, (int)curPoint.y] == 0)
    //        {
    //            float rand = Random.Range(0f, 1f);
    //            if (rand < 0.7f)
    //            {
    //                rooms[(int)curPoint.x, (int)curPoint.y] = 5;
    //            }
    //            else
    //            {
    //                rooms[(int)curPoint.x, (int)curPoint.y] = 6;
    //            }
    //        }
    //        if (second.x > curPoint.x)
    //        {
    //            curPoint = curPoint + new Vector2(1, 0);
    //        }
    //        else if (second.x < curPoint.x)
    //        {
    //            curPoint = curPoint + new Vector2(-1, 0);
    //        }
    //        else if (second.y > curPoint.y)
    //        {
    //            curPoint = curPoint + new Vector2(0, 1);
    //        }
    //        else if (second.y < curPoint.y)
    //        {
    //            curPoint = curPoint + new Vector2(0, -1);
    //        }
    //    }
    //}
}
