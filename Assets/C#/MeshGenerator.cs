﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter map0;

    float fWallHeight;
    float fBumpHeight;
    float fSquareSize;
    int[,] aMap;
    Material oMat;
    Material oMatWalls;

    System.Random random = new System.Random();

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    void Clear()
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    public MeshFilter map0_bg;
    int numStepsX, numStepsY;
    Node[,] oNodes;
    float fUVScaling;
    public void GenerateMeshBackground(int w, int h, float i_fTiling, float i_fBumpHeight, float i_fUVScaling)
    {
        //this uses the same mechanism that produces the fine grained map, but generates a background
        // lowres grid with fewer triangles.
        //this must be run before or after fine grained map, cannot be mixed because of shared variables.

        Clear();

        //create map bg mesh in segments of tiling tiles
        numStepsX = (int)(w / i_fTiling);
        numStepsY = (int)(h / i_fTiling);
        float stepX = (float)w / (float)numStepsX /10.0f;
        float stepY = (float)h / (float)numStepsY /10.0f;

        fUVScaling = i_fUVScaling;
        float fMapWidth = w/10.0f;
        float fMapHeight = h/10.0f;

        oNodes = new Node[numStepsX + 1, numStepsY + 1];

        //set squares
        for (int x = 0; x < numStepsX+1; x++)
        {
            for (int y = 0; y < numStepsY+1; y++)
            {
                Vector3 vPos = new Vector3(-fMapWidth / 2 + x * stepX, -fMapHeight / 2 + y * stepY, 0.0f);
                if (x != 0 && y!=0 && x!= numStepsX && y!= numStepsY) vPos.z = ((float)random.NextDouble() * i_fBumpHeight) - (i_fBumpHeight * 0.5f);
                oNodes[x, y] = new Node(vPos);
            }
        }

        //create mesh
        for (int x = 0; x < numStepsX; x++)
        {
            for (int y = 0; y < numStepsY; y++)
            {
                //MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oTopRight, i_oSquare.oBottomRight, i_oSquare.oBottomLeft);
                MeshFromPoints(oNodes[x, y + 1], oNodes[x + 1, y + 1], oNodes[x + 1, y], oNodes[x, y]);
            }
        }

        //set uv coords for every vertex
        uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = vertices[i].x / fUVScaling;
            float percentY = vertices[i].y / fUVScaling;
            uvs[i] = new Vector2(percentX, percentY);
        }
    }
    public void SetGenerateMeshBackgroundToUnity()
    {
        //set the map bg mesh
        Mesh bgMesh = new Mesh();
        //bgMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //no need to
        bgMesh.SetVertices(vertices);
        bgMesh.SetTriangles(triangles.ToArray(), 0);
        bgMesh.RecalculateNormals();
        bgMesh.uv = uvs;
        map0_bg.mesh = bgMesh;
    }

    int h, w;
    public void GenerateMeshInit(int[,] i_aMap, float i_fSquareSize, float i_fWallHeight, float i_fBumpHeight, Material i_oMat, Material i_oMatWalls)
    {
        aMap = i_aMap;
        fSquareSize = i_fSquareSize;
        fWallHeight = i_fWallHeight;
        fBumpHeight = i_fBumpHeight;
        oMat = i_oMat;
        oMatWalls = i_oMatWalls;
        squareGrid = new SquareGrid(aMap, fSquareSize, fSquareSize);

        squareGrid.Init();

        Clear();
        w = squareGrid.aSquares.GetLength(0);
        h = squareGrid.aSquares.GetLength(1);
    }

    public void GenerateMesh()
    {
        //create map mesh in segments
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if(squareGrid.aSquares[x, y]!=null) TriangulateSquare(squareGrid.aSquares[x, y]);
            }
        }
    }

    public void CreateBumps()
    {
        //create bumps on non outline vertices in the map mesh
        for (int i = 0; i < vertices.Count; i++)
        {
            if (!checkedVertices.Contains(i))
            {
                //get old tile position and then the tile number 
                int iTileX = (int)((vertices[i].x / fSquareSize) + (w / 2));
                int iTileY = (int)((vertices[i].y / fSquareSize) + (h / 2));
                int iTileNum = aMap[iTileY, iTileX];

                //only negative bumps on certain brick walls
                if (iTileNum == 35 || iTileNum == 37)
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z + ((float)random.NextDouble() * 0.5f) * fBumpHeight);
                //no bumps on certain brick walls
                else if (iTileNum == 36 || iTileNum == 38 || iTileNum == 39)
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
                //an full bumps on the rest
                else
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z + ((float)random.NextDouble() - 0.5f) * fBumpHeight);
            }
        }
    }

    Vector2[] uvs;
    public void GenerateUvs()
    {
        //create texture coords
        //int tileAmount = 10;
        uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = vertices[i].x*10;
            float percentY = vertices[i].y*10;
            uvs[i] = new Vector2(percentX, percentY);
        }
    }

    public bool GenerateMeshFinalize(int n)
    {
        if (n == 0)
        {
            //CreateWallMesh() must be done before
            SetCreateWallMeshToUnity();
        }
        else if (n == 1)
        {
            //generate 2d collission
            Generate2DColliders();
        }
        else if (n == 2)
        {
            //set the map mesh
            Mesh mapMesh = new Mesh();
            mapMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mapMesh.SetVertices(vertices);
            mapMesh.SetTriangles(triangles.ToArray(), 0);
            mapMesh.RecalculateNormals();
            mapMesh.uv = uvs;
            map0.mesh = mapMesh;
            //and material
            map0.GetComponent<MeshRenderer>().material = oMat;

            return true;
        }
        return false;
    }

    //must run CalculateMeshOutlines() before this
    List<Vector3> wallVertices = new List<Vector3>();
    List<Vector2> wallUVs = new List<Vector2>();
    List<int> wallTriangles = new List<int>();
    public void CreateWallMesh()
    {
        for (int j=0; j < outlines.Count; j++)
        {
            float uvLastX2 = 0.0f;
            List<int> outline = outlines[j];
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] + Vector3.forward * fWallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] + Vector3.forward * fWallHeight); // bottom right

                //uv for possible texturing (coords from distance along edge)
                float uvX = uvLastX2;
                float uvX2 = uvLastX2 + (vertices[outline[i + 1]] - vertices[outline[i]]).magnitude/6; //about square for 512x256 texture
                uvLastX2 = uvX2;
                wallUVs.Add(new Vector2(uvX, 0.0f));
                wallUVs.Add(new Vector2(uvX2, 0.0f));
                wallUVs.Add(new Vector2(uvX, 1.0f));
                wallUVs.Add(new Vector2(uvX2, 1.0f));

                //haven't figured out why yet, but the outline around the whole map
                // (always j==0) must be created in the opposite order
                if (j==0)
                {
                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 2);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 1);
                }
                else
                {
                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }
        }
    }
    public void SetCreateWallMeshToUnity()
    {
        Mesh wallMesh = new Mesh();
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateNormals();
        wallMesh.uv = wallUVs.ToArray();

        walls.mesh = wallMesh;
        walls.GetComponent<MeshCollider>().sharedMesh = wallMesh;

        //and material
        walls.GetComponent<MeshRenderer>().material = oMatWalls;
    }

    void Generate2DColliders()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }
        gameObject.name = "Map";

        //done before
        //CalculateMeshOutlines();

        for (int j = 0; j < outlines.Count; j++)
        {
            //skip whole map outline in collission, always j==0
            //if (j == 0) continue;

            List<int> outline = outlines[j];

            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].y);
            }
            edgeCollider.points = edgePoints;
        }
    }

    void TriangulateSquare(Square i_oSquare)
    {
        switch (i_oSquare.iConfiguration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(i_oSquare.oCentreLeft, i_oSquare.oCentreBottom, i_oSquare.oBottomLeft);
                break;
            case 2:
                MeshFromPoints(i_oSquare.oBottomRight, i_oSquare.oCentreBottom, i_oSquare.oCentreRight);
                break;
            case 4:
                MeshFromPoints(i_oSquare.oTopRight, i_oSquare.oCentreRight, i_oSquare.oCentreTop);
                break;
            case 8:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oCentreTop, i_oSquare.oCentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(i_oSquare.oCentreRight, i_oSquare.oBottomRight, i_oSquare.oBottomLeft, i_oSquare.oCentreLeft);
                break;
            case 6:
                MeshFromPoints(i_oSquare.oCentreTop, i_oSquare.oTopRight, i_oSquare.oBottomRight, i_oSquare.oCentreBottom);
                break;
            case 9:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oCentreTop, i_oSquare.oCentreBottom, i_oSquare.oBottomLeft);
                break;
            case 12:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oTopRight, i_oSquare.oCentreRight, i_oSquare.oCentreLeft);
                break;
            case 5:
                MeshFromPoints(i_oSquare.oCentreTop, i_oSquare.oTopRight, i_oSquare.oCentreRight, i_oSquare.oCentreBottom, i_oSquare.oBottomLeft, i_oSquare.oCentreLeft);
                break;
            case 10:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oCentreTop, i_oSquare.oCentreRight, i_oSquare.oBottomRight, i_oSquare.oCentreBottom, i_oSquare.oCentreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(i_oSquare.oCentreTop, i_oSquare.oTopRight, i_oSquare.oBottomRight, i_oSquare.oBottomLeft, i_oSquare.oCentreLeft);
                break;
            case 11:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oCentreTop, i_oSquare.oCentreRight, i_oSquare.oBottomRight, i_oSquare.oBottomLeft);
                break;
            case 13:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oTopRight, i_oSquare.oCentreRight, i_oSquare.oCentreBottom, i_oSquare.oBottomLeft);
                break;
            case 14:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oTopRight, i_oSquare.oBottomRight, i_oSquare.oCentreBottom, i_oSquare.oCentreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(i_oSquare.oTopLeft, i_oSquare.oTopRight, i_oSquare.oBottomRight, i_oSquare.oBottomLeft);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].iVertexIndex == -1)
            {
                points[i].iVertexIndex = vertices.Count;
                vertices.Add(points[i].vPos);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.iVertexIndex);
        triangles.Add(b.iVertexIndex);
        triangles.Add(c.iVertexIndex);

        Triangle triangle = new Triangle(a.iVertexIndex, b.iVertexIndex, c.iVertexIndex);
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

    public void CalculateMeshOutlines()
    {
        int iCntTo = vertices.Count;

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
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
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

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
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
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] aSquares;
        int iNodeCountX, iNodeCountY;
        float fMapWidth, fMapHeight;
        float fSquareSizeX, fSquareSizeY;
        ControlNode[,] oControlNodes;
        int[,] oMap;

        public SquareGrid(int[,] i_oMap, float i_fSquareSizeX, float i_fSquareSizeY)
        {
            oMap = i_oMap;
            fSquareSizeX = i_fSquareSizeX;
            fSquareSizeY = i_fSquareSizeY;
            iNodeCountX = i_oMap.GetLength(1);
            iNodeCountY = i_oMap.GetLength(0);
            fMapWidth = iNodeCountX * fSquareSizeX;
            fMapHeight = iNodeCountY * fSquareSizeY;

            oControlNodes = new ControlNode[iNodeCountX, iNodeCountY];
        }

        public void Init()
        {
            for (int x = 0; x < iNodeCountX; x++)
            {
                for (int y = 0; y < iNodeCountY; y++)
                {
                    Vector3 vPos = new Vector3(-fMapWidth / 2 + x * fSquareSizeX + fSquareSizeX / 2, -fMapHeight / 2 + y * fSquareSizeY + fSquareSizeY / 2, 0);
                    oControlNodes[x, y] = new ControlNode(vPos, oMap[y, x] != 0, fSquareSizeX, fSquareSizeY);
                }
            }

            aSquares = new Square[iNodeCountX - 1, iNodeCountY - 1];

            for (int x = 0; x < iNodeCountX - 1; x++)
            {
                for (int y = 0; y < iNodeCountY - 1; y++)
                {
                    aSquares[x, y] = new Square(oControlNodes[x, y + 1], oControlNodes[x + 1, y + 1], oControlNodes[x + 1, y], oControlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode oTopLeft, oTopRight, oBottomRight, oBottomLeft;
        public Node oCentreTop, oCentreRight, oCentreBottom, oCentreLeft;
        public int iConfiguration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            oTopLeft = _topLeft;
            oTopRight = _topRight;
            oBottomRight = _bottomRight;
            oBottomLeft = _bottomLeft;

            oCentreTop = oTopLeft.oRight;
            oCentreRight = oBottomRight.oAbove;
            oCentreBottom = oBottomLeft.oRight;
            oCentreLeft = oBottomLeft.oAbove;

            if (oTopLeft.bActive)
                iConfiguration += 8;
            if (oTopRight.bActive)
                iConfiguration += 4;
            if (oBottomRight.bActive)
                iConfiguration += 2;
            if (oBottomLeft.bActive)
                iConfiguration += 1;
        }
    }

    public class Node
    {
        public Vector3 vPos;
        public int iVertexIndex = -1;

        public Node(Vector3 _pos)
        {
            vPos = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool bActive;
        public Node oAbove, oRight;

        public void Set(float squareSizeX, float squareSizeY)
        {
            oAbove.vPos = vPos + Vector3.up * squareSizeY * 0.5f;
            oRight.vPos = vPos + Vector3.right * squareSizeX * 0.5f;
        }

        public ControlNode(Vector3 _pos, bool _active, float squareSizeX, float squareSizeY) : base(_pos)
        {
            bActive = _active;
            oAbove = new Node(vPos + Vector3.up * squareSizeY * 0.5f);
            oRight = new Node(vPos + Vector3.right * squareSizeX * 0.5f);
        }
    }
}
