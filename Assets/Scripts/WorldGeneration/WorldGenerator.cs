using System.Collections.Generic;
using UnityEngine;
using System;

//[System.Serializable]
//[CreateAssetMenu(fileName = "New WorldGeneration", menuName = "Algorithm/New WorldGeneration")]
public class WorldGenerator: MonoBehaviour
{
    /**
     * Air = 0
     * Wall = 1
     * Spawn = 2
     * Boss = 3
     * Fight = 4
     * Chest = 5
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

    //public Vector2 worldSize;
    //public int[,] rooms;
    //public int eliteCount;
    //public int chestCount;
    //public int fightCount;
    //public int eventCount;
    //public int shopCount;
    //public List<Vector2> keyLocs;
    //public float visualDelay;

    //public static WorldGenerator Instance;

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    MeshGenerator meshGen;

    [SerializeField]
    private int spawnLoc; // 0 = tl, 1 = tr, 2 = bl, 3 = br
    [SerializeField]
    private Coord spawnCoord;
    [SerializeField]
    private int bossLoc; // same as spawnloc
    [SerializeField]
    private Coord bossCoord;
    [SerializeField]
    private int fightCount;
    [SerializeField]
    private List<Coord> fightLoc;
    [SerializeField]
    private int chestCount;
    [SerializeField]
    private List<Coord> chestLoc;

    public Vector2Int fightMinMax;
    public Vector2Int chestMinMax;
    public int spawnRadius = 4;
    public int bossRadius = 6;
    public int fightRadius = 5;
    public int chestRadius = 1;

    public GameObject playerPrefab;
    public GameObject[] bossPrefab;
    public GameObject bossPortalPrefab;
    public EnemyHolder enemyHolder;
    public GameObject[] enemyPrefab;
    public GameObject chestPrefab;
    public Item[] itemsList;

    public GameObject player;
    public GameObject boss;
    public int bossIdx;

    [Range(0, 100)]
    public int randomFillPercent;
    [Range(0, 50)]
    public int smoothIteration;
    public int borderSize = 5;
    public int wallThresholdSize = 50;
    public int roomThresholdSize = 50;

    int[,] map;

    //private void Start()
    //{
    //    GenerateMap();
    //}

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.L))
    //    {
    //        GenerateMap();
    //    }
    //}

    //private void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(this);
    //    }
    //    else
    //    {
    //        Instance = this;
    //    }
    //}

    private void Awake()
    {
        meshGen = GetComponent<MeshGenerator>();
    }

    public void GenerateMap(bool spawnEntity = true)
    {
        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0f, 1f).ToString();
        }

        System.Random rnd = new System.Random(seed.GetHashCode());

        map = new int[width, height];
        RandomFillMap(rnd);

        for (int i = 0; i < smoothIteration / 2; i++)
        {
            SmoothMap();
        }

        int[,] grassMap = (int[,])map.Clone();

        for (int i = 0; i < smoothIteration / 2; i++)
        {
            SmoothMap();
        }

        GenerateStructures();
        ProcessMap();

        GameObject[] ehs = GameObject.FindGameObjectsWithTag("EnemyHolder");
        foreach (var eh in ehs)
        {
            Destroy(eh);
        }

        if (spawnEntity)
        {
            foreach (var loc in fightLoc)
            {
                SpawnEnemies(rnd, loc);
            }
            foreach (var loc in chestLoc)
            {
                SpawnChest(rnd, loc);
            }
            calcBoss(rnd);
            SpawnBossPortal();
            SpawnPlayer();
        }

        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        int[,] borderedMapGrass = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                    borderedMapGrass[x, y] = grassMap[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                    borderedMapGrass[x, y] = 1;
                }
            }
        }

        meshGen.GenerateMesh(borderedMap, 1f, borderedMapGrass);
    }

    void GenerateStructures()
    {
        // Generate Spawn
        DrawCircle(spawnCoord, spawnRadius, 2, 0, 1);

        // Generate Boss
        DrawCircle(bossCoord, bossRadius, 3, 0, 1);

        // Generate Fights
        foreach (var fight in fightLoc)
        {
            DrawCircle(fight, fightRadius, 4, 0, 1);
        }

        // Generate Chest
        foreach (var chest in chestLoc)
        {
            DrawCircle(chest, chestRadius, 5, 0, 1);
        }
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }

        List<List<Coord>> structureRegions = GetRegions(0,2,3,4,5);
        List<Room> survivingRooms = new List<Room>();
        foreach (List<Coord> structureRegion in structureRegions)
        {
            //foreach (var item in structureRegion)
            //{
            //    Debug.Log(item);
            //}
            survivingRooms.Add(new Room(structureRegion, map));
        }

        survivingRooms.Sort();
        //for (int i = 0; i < survivingRooms.Count; i++)
        //{
        //    Debug.Log(survivingRooms[i]);
        //}
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessableFromMainRoom = true;
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessableFromMainRoom)
                {
                    roomListB.Add(room);
                } else
                {
                    roomListA.Add(room);
                }
            }
        } else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.black, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 2, 0, 1);
        }
    }

    void DrawCircle(Coord c, int r, int val, params int[] replaceVal)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x*x + y*y < r*r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        if (Array.Exists(replaceVal, value => value.Equals(map[drawX, drawY])))
                            map[drawX, drawY] = val;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            } else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                } else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, -height / 2 + .5f + tile.tileY, -2);
    }

    List<List<Coord>> GetRegions(params int[] tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && Array.Exists(tileType, value => value.Equals(map[x, y])))
                {
                    List<Coord> newRegion = GetRegionTiles(x, y, tileType);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY, params int[] tileType)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && Array.Exists(tileType, value => value.Equals(map[x, y])))
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void RandomFillMap(System.Random rnd)
    {
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width-1 || y == 0 || y == height-1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (rnd.Next(0, 100) < randomFillPercent)? 1 : 0;
                }
            }
        }

        Dictionary<Coord, int> existingStructures = new Dictionary<Coord, int>();

        spawnLoc = rnd.Next(0, 4);
        switch (spawnLoc)
        {
            case 0:
                spawnCoord = new Coord(spawnRadius, height - spawnRadius);
                break;
            case 1:
                spawnCoord = new Coord(width - spawnRadius, height - spawnRadius);
                break;
            case 2:
                spawnCoord = new Coord(spawnRadius, spawnRadius);
                break;
            case 3:
                spawnCoord = new Coord(width - spawnRadius, spawnRadius);
                break;
        }
        existingStructures.Add(spawnCoord, spawnRadius);

        bossLoc = rnd.Next(0, 4);
        while(bossLoc == spawnLoc)
        {
            bossLoc = rnd.Next(0, 4);
        }
        switch (bossLoc)
        {
            case 0:
                bossCoord = new Coord(bossRadius, height - bossRadius);
                break;
            case 1:
                bossCoord = new Coord(width - bossRadius, height - bossRadius);
                break;
            case 2:
                bossCoord = new Coord(bossRadius, bossRadius);
                break;
            case 3:
                bossCoord = new Coord(width - bossRadius, bossRadius);
                break;
        }
        existingStructures.Add(bossCoord, bossRadius);

        fightCount = rnd.Next(fightMinMax.x, fightMinMax.y + 1);
        chestCount = rnd.Next(chestMinMax.x, chestMinMax.y + 1);
        fightLoc.Clear();
        chestLoc.Clear();

        for (int i = 0; i < fightCount; i++)
        {
            Coord tempCoord = new Coord(rnd.Next(fightRadius, width - fightRadius), rnd.Next(fightRadius, height - fightRadius));
            bool fit = true;
            foreach (var item in existingStructures)
            {
                if (CircleOverlap(item.Key, item.Value, tempCoord, fightRadius + 1))
                {
                    fit = false;
                    break;
                }
            }
            while (!fit)
            {
                tempCoord = new Coord(rnd.Next(fightRadius, width - fightRadius), rnd.Next(fightRadius, height - fightRadius));
                fit = true;
                foreach (var item in existingStructures)
                {
                    if (CircleOverlap(item.Key, item.Value, tempCoord, fightRadius + 1))
                    {
                        fit = false;
                        break;
                    }
                }
            }
            existingStructures.Add(tempCoord, fightRadius);
            fightLoc.Add(tempCoord);
        }

        for (int i = 0; i < chestCount; i++)
        {
            Coord tempCoord = new Coord(rnd.Next(chestRadius, width - chestRadius), rnd.Next(chestRadius, height - chestRadius));
            bool fit = true;
            foreach (var item in existingStructures)
            {
                if (CircleOverlap(item.Key, item.Value, tempCoord, chestRadius + 1))
                {
                    fit = false;
                    break;
                }
            }
            while (!fit)
            {
                tempCoord = new Coord(rnd.Next(chestRadius, width - chestRadius), rnd.Next(chestRadius, height - chestRadius));
                fit = true;
                foreach (var item in existingStructures)
                {
                    if (CircleOverlap(item.Key, item.Value, tempCoord, chestRadius + 1))
                    {
                        fit = false;
                        break;
                    }
                }
            }
            existingStructures.Add(tempCoord, chestRadius);
            chestLoc.Add(tempCoord);
        }
    }

    bool CircleOverlap(Coord circleA, int rA, Coord circleB, int rB)
    {
        return Coord.Distance(circleA, circleB) < (rA + rB);
    }

    void SmoothMap()
    {
        int[,] tempMap = (int[,])map.Clone(); 
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    tempMap[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    tempMap[x, y] = 0;
                }
            }
        }
        map = tempMap;
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    public void ClearMap()
    {
        GameObject[] ehs = GameObject.FindGameObjectsWithTag("EnemyHolder");
        foreach (var eh in ehs)
        {
            Destroy(eh);
        }
        GameObject[] chests = GameObject.FindGameObjectsWithTag("Chest");
        foreach (var chest in chests)
        {
            Destroy(chest);
        }

        meshGen.ClearMap();
    }

    [Serializable]
    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }

        public Vector3 ToWorldPos(int width, int height, float squareSize)
        {
            return new Vector3(-width / 2 + tileX * squareSize + squareSize / 2, -height / 2 + tileY * squareSize + squareSize / 2, 0);
        }

        public static float Distance(Coord A, Coord B)
        {
            return Mathf.Sqrt((A.tileX - B.tileX) * (A.tileX - B.tileX) + (A.tileY - B.tileY) * (A.tileY - B.tileY));
        }

        public override string ToString() => "[" + tileX + ", " + tileY + "]";
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessableFromMainRoom;
        public bool isMainRoom;

        public Room() { }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1 ; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if ((x == tile.tileX || y == tile.tileY) &&
                            x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessableFromMainRoom)
            {
                isAccessableFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.isAccessableFromMainRoom = true;
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessableFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessableFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public int CompareTo(Room other)
        {
            return other.roomSize.CompareTo(roomSize);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }
    }

    private void SpawnPlayer()
    {
        SpawnPlayer(spawnCoord.ToWorldPos(width, height, 1f));
    }

    public void SpawnPlayer(Vector3 location)
    {
        if (player != null) return;
        player = Instantiate(playerPrefab, location, Quaternion.identity);
        Debug.Log("Player instantiated at: " + location);
    }

    private void SpawnEnemies(System.Random rnd, Coord location)
    {
        EnemyHolder eh = Instantiate(enemyHolder, location.ToWorldPos(width, height, 1f), Quaternion.identity);
        int enemyCount = rnd.Next(2, 5);
        for (int i = 0; i < enemyCount; i++)
        {
            int idx = rnd.Next(enemyPrefab.Length);
            GameObject enemy = Instantiate(enemyPrefab[idx], eh.transform.position, Quaternion.identity, eh.transform);
            eh.Enemies.Add(enemy.GetComponent<EnemyController>());
        }
    }

    private void SpawnChest(System.Random rnd, Coord location)
    {
        Instantiate(chestPrefab, location.ToWorldPos(width, height, 1f), Quaternion.identity);
    }

    private void calcBoss(System.Random rnd)
    {
        bossIdx = rnd.Next(bossPrefab.Length);
    }

    public void SpawnBoss(Vector3 location)
    {
        boss = Instantiate(bossPrefab[bossIdx], location, Quaternion.identity);
        Debug.Log("Boss instantiated at: " + location);
    }

    private void SpawnBossPortal()
    {
        Instantiate(bossPortalPrefab, bossCoord.ToWorldPos(width, height, 1f), Quaternion.identity);
    }
}
