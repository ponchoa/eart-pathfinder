using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public int sizeX, sizeY;
    public GameObject tilePrefab;
    public bool drawInSteps;
    TileData[,] mapData;
    GameObject[,] tileMap;
    bool cursorWall;
    public Vector2Int StartCoord, EndCoord;
    bool isOnPath = false;

    public TileData this[int x, int y]
    {
        get { return mapData[x, y]; }
        set { mapData[x, y] = value; }
    }
    public TileData StartTile
    {
        get { return mapData[StartCoord.x, StartCoord.y]; }
        set { mapData[StartCoord.x, StartCoord.y] = value; }
    }
    public TileData EndTile
    {
        get { return mapData[EndCoord.x, EndCoord.y]; }
        set { mapData[EndCoord.x, EndCoord.y] = value; }
    }

    private void Start()
    {
        mapData = new TileData[sizeX, sizeY];
        tileMap = new GameObject[sizeX, sizeY];
        StartCoord = new Vector2Int(-1, -1);
        EndCoord = new Vector2Int(-1, -1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                mapData[x, y] = new TileData();
                mapData[x, y].CostMult = 1f;
                mapData[x, y].IsWall = false;
                mapData[x, y].x = x;
                mapData[x, y].y = y;
                Vector3 pos = new Vector3(x - sizeX / 2f + .5f, y - sizeY / 2f + .5f, 0f);
                tileMap[x, y] = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                mapData[x, y].Parent = null;
            }
        }

        ResetMap();
    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = (int)Mathf.Floor(mousePos.x + sizeX / 2f);
        int y = (int)Mathf.Floor(mousePos.y + sizeY / 2f);
        bool isStartOrEnd = StartCoord == new Vector2Int(x, y) || EndCoord == new Vector2Int(x, y);

        if (x >= 0 && y >= 0 && x < sizeX && y < sizeY)
        {
            if (Input.GetMouseButtonDown(0))
            {
                cursorWall = !mapData[x, y].IsWall;
            }
            if (Input.GetMouseButton(0))
            {
                if (mapData[x, y].IsWall != cursorWall)
                {
                    mapData[x, y].IsWall = cursorWall;
                    if (!cursorWall)
                        mapData[x, y].CostMult = Mathf.Clamp(mapData[x, y].CostMult, 1f, 3f);
                    if (!isStartOrEnd)
                        SetColor(x, y, cursorWall ? Color.black : CostToColor(mapData[x, y].CostMult));
                }
            }
            if (Input.GetMouseButton(1))
            {
                mapData[x, y].IsWall = false;
                mapData[x, y].CostMult = 1f;
                if (!isStartOrEnd)
                    SetColor(x, y, Color.white);
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (!mapData[x, y].IsWall || mapData[x, y].CostMult < 1f || mapData[x, y].CostMult > 3f)
                {
                    mapData[x, y].IsWall = false;
                    mapData[x, y].CostMult += .1f;
                    if (!isStartOrEnd)
                        SetColor(x, y, CostToColor(mapData[x, y].CostMult));
                    if (mapData[x, y].CostMult > 3f)
                    {
                        mapData[x, y].IsWall = true;
                        if (!isStartOrEnd)
                            SetColor(x, y, Color.black);
                    }

                }

            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if (!mapData[x, y].IsWall || mapData[x, y].CostMult < 1f || mapData[x, y].CostMult > 3f)
                {
                    mapData[x, y].IsWall = false;
                    mapData[x, y].CostMult -= .1f;
                    if (!isStartOrEnd)
                        SetColor(x, y, CostToColor(mapData[x, y].CostMult));
                    if (mapData[x, y].CostMult < 1f)
                    {
                        mapData[x, y].IsWall = true;
                        if (!isStartOrEnd)
                            SetColor(x, y, Color.black);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (StartCoord.x >= 0 && StartCoord.y >= 0)
                    SetColor(StartCoord.x, StartCoord.y, mapData[StartCoord.x, StartCoord.y].IsWall ? Color.black
                        : CostToColor(mapData[StartCoord.x, StartCoord.y].CostMult));
                StartCoord = new Vector2Int(x, y);
                ResetPathData();
                SetColor(x, y, Color.green);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (EndCoord.x >= 0 && EndCoord.y >= 0)
                    SetColor(EndCoord.x, EndCoord.y, mapData[EndCoord.x, EndCoord.y].IsWall ? Color.black
                    : CostToColor(mapData[EndCoord.x, EndCoord.y].CostMult));
                EndCoord = new Vector2Int(x, y);
                ResetPathData();
                SetColor(x, y, Color.yellow);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetMap();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (drawInSteps)
                {
                    isOnPath = !PathfinderPrep.FindPathInSteps(this, !isOnPath);
                }
                else
                {
                    Pathfinder.FindPath(this);
                }
            }

            mapData[x, y].CostMult = Mathf.Clamp(mapData[x, y].CostMult, 0.9f, 3.1f);
        }
        UpdateTexts();
    }

    void UpdateTexts()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                foreach (Transform child in tileMap[x, y].transform)
                {
                    switch (child.name)
                    {
                        case "CostMult":
                            child.GetComponent<TextMeshPro>().text = mapData[x, y].CostMult.ToString("F1");
                            break;
                        case "GScore":
                            child.GetComponent<TextMeshPro>().text = mapData[x, y].GScore.ToString();
                            break;
                        case "HScore":
                            child.GetComponent<TextMeshPro>().text = mapData[x, y].HScore.ToString();
                            break;
                        case "FScore":
                            child.GetComponent<TextMeshPro>().text = mapData[x, y].FScore.ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public void ResetMap()
    {
        StartCoord = new Vector2Int(-1, -1);
        EndCoord = new Vector2Int(-1, -1);
        isOnPath = false;
        ResetPathData();
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                mapData[x, y].CostMult = 1f;
                mapData[x, y].IsWall = false;
                mapData[x, y].Parent = null;
                SetColor(x, y, Color.white);
            }
        }
    }

    public void ResetPathData()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                mapData[x, y].GScore = 0;
                mapData[x, y].HScore = 0;
                mapData[x, y].Parent = null;
                if (StartCoord == new Vector2Int(-1, -1) || EndCoord == new Vector2Int(-1, -1) || (mapData[x, y] != StartTile && mapData[x, y] != EndTile))
                    SetColor(x, y, mapData[x, y].IsWall ? Color.black : CostToColor(mapData[x, y].CostMult));
                else if (mapData[x, y] == StartTile)
                    SetColor(x, y, Color.green);
            }
        }
    }

    public void SetColor(int x, int y, Color color)
    {
        tileMap[x, y].GetComponent<SpriteRenderer>().material.color = color;
    }
    public void SetColor(TileData tile, Color color)
    {
        SetColor(tile.x, tile.y, color);
    }
    public Color CostToColor(float cost)
    {
        if (cost == 1f) //Road
            return Color.white;
        if (cost > 1f && cost <= 1.5f) //Grass
            return new Color(.5f, 1f, .5f);
        if (cost > 1.5 && cost <= 2f) //Sand
            return new Color(1f, .85f, .5f);
        if (cost > 2f && cost <= 2.5f) //Mountain
            return new Color(.6f, .3f, 0f);
        if (cost > 2.5f && cost <= 3f) //Water
            return new Color(.7f, .85f, .9f);
        return Color.black;
    }
}

public class TileData : IComparable<TileData>
{
    public int GScore, HScore;
    public int FScore
    {
        get { return GScore + HScore; }
    }
    public float CostMult;
    public bool IsWall;
    public int x, y;
    public TileData Parent;

    public int CompareTo(TileData other)
    {
        if (FScore != other.FScore)
            return FScore - other.FScore;
        return HScore - other.HScore;
    }
}
