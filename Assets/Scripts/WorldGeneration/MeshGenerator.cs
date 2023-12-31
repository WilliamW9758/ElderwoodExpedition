using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls;
    public Tilemap topMap;
    public Tilemap wallMap;
    public Tilemap topShadowMap;
    public Tilemap groundMap;
    public Tilemap groundGrassMap;
    public Tile[] topTiles;
    public Tile[] wallTiles;
    public Tile[] groundTiles;
    public Tile[] grassTiles;
    List<Vector3> vertices;
    List<int> triangles;

    public float wallHeight;
    public bool generateWallMesh;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize, int[,] grassMap)
    {
        ClearMap();

        int[,] treeMap = ImageProcessing.ImageProcessing.Erode(map, 2);
        int[,] shadowMap = ImageProcessing.ImageProcessing.Dilate(map, 3);
        grassMap = ImageProcessing.ImageProcessing.Dilate(grassMap, 2);

        squareGrid = new SquareGrid(map, squareSize);
        SquareGrid treeSquareGrid = new SquareGrid(treeMap, squareSize);
        SquareGrid shadowSquareGrid = new SquareGrid(shadowMap, squareSize);
        SquareGrid grassSquareGrid = new SquareGrid(grassMap, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1) - 1; y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
                wallMap.SetTile(Vector3Int.FloorToInt(squareGrid.squares[x, y].bottomLeft.position), wallTiles[squareGrid.squares[x, y].configuration]);
                PlaceGroundTiles(map, x, y, squareGrid.squares[x, y]);
                if (treeSquareGrid.squares[x, y].configuration != 0)
                {
                    topMap.SetTile(Vector3Int.FloorToInt(treeSquareGrid.squares[x, y].bottomLeft.position), topTiles[treeSquareGrid.squares[x, y].configuration]);
                }
                if (shadowSquareGrid.squares[x, y].configuration != 0)
                {
                    topShadowMap.SetTile(Vector3Int.FloorToInt(shadowSquareGrid.squares[x, y].bottomLeft.position), topTiles[shadowSquareGrid.squares[x, y].configuration]);
                }
                if (grassSquareGrid.squares[x, y].configuration != 0)
                {
                    groundGrassMap.SetTile(Vector3Int.FloorToInt(grassSquareGrid.squares[x, y].bottomLeft.position), grassTiles[grassSquareGrid.squares[x, y].configuration]);
                }
            }
        }

        //for (int x = 0; x < map.GetLength(0); x++)
        //{
        //    for (int y = 0; y < map.GetLength(1); y++)
        //    {
        //        if (map[x, y] == 1)
        //        {
        //            topMap.SetTile(new Vector3Int(x - map.GetLength(0) / 2, y - map.GetLength(1) / 2, 0), topTile);
        //        }
        //    }
        //}

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //int tileAmount = 10;

        // Texture TBD
        //Vector2[] uvs = new Vector2[vertices.Count];
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
        //    float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].y) * tileAmount;
        //    uvs[i] = new Vector2(percentX, percentY);
        //}
        //mesh.uv = uvs;


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Generate2DColliders();
        if (generateWallMesh)
        {
            CreateWallMesh();
        }
    }

    void CreateWallMesh()
    {
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count-1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left vertex
                wallVertices.Add(vertices[outline[i+1]]); // right vertex
                wallVertices.Add(vertices[outline[i]] + Vector3.forward * wallHeight); // top left vertex
                wallVertices.Add(vertices[outline[i + 1]] + Vector3.forward * wallHeight); // top right vertex

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
    }

    void Generate2DColliders()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = vertices[outline[i]];
            }
            edgeCollider.points = edgePoints;
        }
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 point
            case 1:
                MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;

            // 2 points
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 3 points
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 4 points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void PlaceGroundTiles(int[,] map, int x, int y, Square square)
    {
        //switch(map[x, y])
        //{
        //    case 2:
        //        groundMap.SetTile(Vector3Int.FloorToInt(square.bottomLeft.position), groundTiles[0]);
        //        break;
        //    case 3:
        //        groundMap.SetTile(Vector3Int.FloorToInt(square.bottomLeft.position), groundTiles[1]);
        //        break;
        //    case 4:
        //        groundMap.SetTile(Vector3Int.FloorToInt(square.bottomLeft.position), groundTiles[2]);
        //        break;
        //    case 5:
        //        groundMap.SetTile(Vector3Int.FloorToInt(square.bottomLeft.position), groundTiles[3]);
        //        break;
        //}
        groundMap.SetTile(Vector3Int.FloorToInt(square.bottomLeft.position), groundTiles[4]);
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutLineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutLineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
                sharedTriangleCount++;
            if (sharedTriangleCount > 1)
                break;
        }

        return sharedTriangleCount == 1;
    }

    public void ClearMap()
    {
        outlines.Clear();
        checkedVertices.Clear();
        triangleDictionary.Clear();

        Debug.Log(topMap);
        topMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        topShadowMap.ClearAllTiles();
        groundMap.ClearAllTiles();
        groundGrassMap.ClearAllTiles();
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            vertices = new int[] { vertexIndexA, vertexIndexB, vertexIndexC };
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA ||
                vertexIndex == vertexIndexB ||
                vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2, 0);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX-1; x++)
            {
                for (int y = 0; y < nodeCountY-1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

            if (topLeft.isActive)
                configuration += 8;
            if (topRight.isActive)
                configuration += 4;
            if (bottomRight.isActive)
                configuration += 2;
            if (bottomLeft.isActive)
                configuration += 1;
        }

    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool isActive;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            isActive = _active;
            above = new Node(position + Vector3.up * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
