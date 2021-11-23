using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathfinderPrep
{
    const int STRAIGHT = 10;
    const int DIAGONAL = 14;

    static int HeuristicDistance(TileData start, TileData end)
    {
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        return DIAGONAL * Mathf.Min(dx, dy) + STRAIGHT * Mathf.Abs(dx - dy);
    }
    static int DistanceBetweenNeighbours(TileData start, TileData end)
    {
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        if (dx > 1 || dy > 1)
            return 0;

        if (start.x == end.x || start.y == end.y) //Straight
        {
            return (int)(STRAIGHT * end.CostMult);
        }
        else
        {
            return (int)(DIAGONAL * end.CostMult);
        }
    }

    static List<TileData> openSet;
    static List<TileData> closedSet;

    static bool CheckMap(MapHandler map)
    {
        if (map == null)
            return false;
        if (map.sizeX <= 0 || map.sizeY <= 0)
            return false;
        if (map.StartCoord.x < 0 || map.StartCoord.x >= map.sizeX || map.StartCoord.y < 0 || map.StartCoord.y >= map.sizeY)
            return false;
        if (map.EndCoord.x < 0 || map.EndCoord.x >= map.sizeX || map.EndCoord.y < 0 || map.EndCoord.y >= map.sizeY)
            return false;
        if (map.StartCoord == map.EndCoord)
            return false;
        if (map[map.EndCoord.x, map.EndCoord.y].IsWall)
            return false;

        return true;
    }

    public static void FindPath(MapHandler map)
    {
        map.ResetPathData();
        if (CheckMap(map))
        {
            List<TileData> open = new List<TileData>();
            List<TileData> closed = new List<TileData>();

            open.Add(map.StartTile);

            while (open.Count > 0)
            {
                open.Sort();
                TileData currTile = open.First();
                map.SetColor(currTile, Color.green);
                open.Remove(currTile);
                closed.Add(currTile);

                if (currTile == map.EndTile) //Path Found
                {
                    TracePath(map, closed);
                    return;
                }

                for (int x = Mathf.Max(0, currTile.x - 1); x <= Mathf.Min(map.sizeX - 1, currTile.x + 1); x++)
                {
                    for (int y = Mathf.Max(0, currTile.y - 1); y <= Mathf.Min(map.sizeY - 1, currTile.y + 1); y++)
                    {
                        if (!map[x, y].IsWall && !closed.Contains(map[x, y]))
                        {
                            int newGScore = currTile.GScore + DistanceBetweenNeighbours(currTile, map[x, y]);
                            if (!open.Contains(map[x, y]))
                            {
                                map[x, y].GScore = newGScore;
                                map[x, y].HScore = HeuristicDistance(map[x, y], map.EndTile);
                                map[x, y].Parent = currTile;
                                open.Add(map[x, y]);
                                map.SetColor(map[x, y], Color.blue);
                            }
                            else
                            {
                                if (map[x, y].GScore > newGScore)
                                {
                                    map[x, y].GScore = newGScore;
                                    map[x, y].Parent = currTile;
                                }
                            }
                        }
                    }
                }
            }

            foreach (TileData tile in closed)
            {
                map.SetColor(tile, Color.red);
            }
        }
    }

    public static bool FindPathInSteps(MapHandler map, bool isFirst = false)
    {
        if (CheckMap(map))
        {
            if (isFirst)
            {
                map.ResetPathData();
                openSet = new List<TileData>();
                closedSet = new List<TileData>();

                openSet.Add(map.StartTile);
                map.SetColor(map.StartTile, Color.blue);
            }
            else
            {
                if (openSet.Count <= 0)
                {
                    foreach (TileData tile in closedSet)
                    {
                        map.SetColor(tile, Color.red);
                    }
                    
                    return true;
                }
                openSet.Sort();
                TileData currTile = openSet.First();
                map.SetColor(currTile, Color.green);
                openSet.Remove(currTile);
                closedSet.Add(currTile);

                if (currTile == map.EndTile) //Path Found
                {
                    TracePath(map, closedSet);
                    return true;
                }

                for (int x = Mathf.Max(0, currTile.x - 1); x <= Mathf.Min(map.sizeX - 1, currTile.x + 1); x++)
                {
                    for (int y = Mathf.Max(0, currTile.y - 1); y <= Mathf.Min(map.sizeY - 1, currTile.y + 1); y++)
                    {
                        if (!map[x, y].IsWall && !closedSet.Contains(map[x, y]))
                        {
                            int newGScore = currTile.GScore + DistanceBetweenNeighbours(currTile, map[x, y]);
                            if (!openSet.Contains(map[x, y]))
                            {
                                map[x, y].GScore = newGScore;
                                map[x, y].HScore = HeuristicDistance(map[x, y], map.EndTile);
                                map[x, y].Parent = currTile;
                                openSet.Add(map[x, y]);
                                map.SetColor(map[x, y], Color.blue);
                            }
                            else
                            {
                                if (map[x, y].GScore > newGScore)
                                {
                                    map[x, y].GScore = newGScore;
                                    map[x, y].Parent = currTile;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public static void TracePath(MapHandler map, List<TileData> closed)
    {
        if (closed != null && closed.Count > 0)
        {
            foreach (TileData tile in closed)
            {
                map.SetColor(tile, Color.red);
            }
        }
        TileData currTile = map.EndTile;
        while (currTile.Parent != null)
        {
            map.SetColor(currTile.x, currTile.y, Color.yellow);
            currTile = currTile.Parent;
        }
    }
}
