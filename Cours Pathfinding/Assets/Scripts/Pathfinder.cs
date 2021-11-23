using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    const int STRAIGHT = 10;
    const int DIAGONAL = 14;

    static int DistanceBetweenNeighbours(TileData start, TileData end)
    {
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        if (dx > 1 || dy > 1)
            return int.MaxValue;
        else if (dx == 0 && dy == 0)
            return 0;

        if (dx == 0 || dy == 0)
            return (int)(STRAIGHT * end.CostMult);
        return (int)(DIAGONAL * end.CostMult);
    }
    static int HeuristicDistance(TileData start, TileData end)
    {
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        return DIAGONAL * Mathf.Min(dx, dy) + STRAIGHT * Mathf.Abs(dx - dy);
    }

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
        if (map.StartTile.IsWall || map.EndTile.IsWall)
            return false;

        return true;
    }

    public static void FindPath(MapHandler map)
    {
        map.ResetPathData(); // Remettre les scores des Tiles à 0
        if (CheckMap(map))
        {
            List<TileData> open = new List<TileData>(); // Liste des Tiles à observer
            List<TileData> closed = new List<TileData>(); // Liste des Tiles déjà observées

            open.Add(map.StartTile); // On ajoute la Tile de départ

            while (open.Count > 0) // Si open est vide, il n'y a aucun chemin possible
            {
                open.Sort(); // On trie la liste par les FScore de chaque Tile
                TileData currTile = open.First(); // On observe la plus optimale
                closed.Add(currTile);
                open.Remove(currTile);

                if (currTile == map.EndTile) // On a trouvé la Tile d'arrivée
                {
                    TracePath(map, closed);
                    return;
                }

                // On doit observer les voisins de la Tile en cours
                for (int x = Mathf.Max(0, currTile.x - 1); x <= Mathf.Min(currTile.x + 1, map.sizeX - 1); x++)
                {
                    for (int y = Mathf.Max(0, currTile.y - 1); y <= Mathf.Min(currTile.y + 1, map.sizeY - 1); y++)
                    {
                        if (map[x, y].IsWall || closed.Contains(map[x, y]))
                            continue;
                        int newGScore = currTile.GScore + DistanceBetweenNeighbours(currTile, map[x, y]);
                        if (open.Contains(map[x, y]))
                        {
                            if (map[x, y].GScore > newGScore)
                            {
                                map[x, y].GScore = newGScore;
                                map[x, y].Parent = currTile;
                            }
                        }
                        else
                        {
                            open.Add(map[x, y]);
                            map.SetColor(map[x, y], Color.blue);
                            map[x, y].GScore = newGScore;
                            map[x, y].HScore = HeuristicDistance(map[x, y], map.EndTile);
                            map[x, y].Parent = currTile;
                        }
                    }
                }
            }
            if (closed != null && closed.Count > 0)
            {
                foreach (TileData tile in closed)
                {
                    map.SetColor(tile, Color.red);
                }
            }
        }
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
        while (currTile != null)
        {
            map.SetColor(currTile, Color.yellow);
            currTile = currTile.Parent;
        }
    }
}
