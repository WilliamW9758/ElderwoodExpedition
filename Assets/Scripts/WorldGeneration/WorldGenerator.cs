using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

//[System.Serializable]
//[CreateAssetMenu(fileName = "New WorldGeneration", menuName = "Algorithm/New WorldGeneration")]
public class WorldGenerator: MonoBehaviour
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

    //public Vector2 worldSize;
    //public int[,] rooms;
    //public Vector2 eliteMinMax;
    //public Vector2 chestMinMax;
    //public Vector2 shopMinMax;
    //public int eliteCount;
    //public int chestCount;
    //public int fightCount;
    //public int eventCount;
    //public int shopCount;
    //public List<Vector2> keyLocs;
    //public float visualDelay;

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;
    [Range(0, 50)]
    public int smoothIteration;
    public int borderSize = 5;
    public int wallThresholdSize = 50;
    public int roomThresholdSize = 50;

    int[,] map;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < smoothIteration; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1f);
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
        List<Room> survivingRooms = new List<Room>();
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
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
            DrawCirle(c, 2);
        }
    }

    void DrawCirle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x*x + y*y <= r*r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
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

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
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

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

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
                        if (mapFlags[x, y] == 0 && map[x,y] == tileType)
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

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0f, 1f).ToString();
        }

        System.Random rnd = new System.Random(seed.GetHashCode());

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

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
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
                        if (x == tile.tileX || y == tile.tileY)
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
}
