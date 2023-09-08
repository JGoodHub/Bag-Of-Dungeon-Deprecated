using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPathfinding
{
    private List<DungeonTile> _tiles;

    public DungeonPathfinding(List<DungeonTile> tiles)
    {
        _tiles = tiles;
    }

    public List<DungeonTile> GetShortestPath(DungeonTile start, DungeonTile end)
    {
        List<DungeonTile> path = new List<DungeonTile>();
        Dictionary<DungeonTile, int> distanceMap = new Dictionary<DungeonTile, int>();

        Queue<DungeonTile> searchQueue = new Queue<DungeonTile>();
        HashSet<DungeonTile> searchedTilesSet = new HashSet<DungeonTile>();

        searchQueue.Enqueue(start);
        distanceMap.Add(start, 0);

        // Calculate the max distance from the start tile to all other tiles in the dungeon
        while (searchQueue.Count > 0)
        {
            DungeonTile searchTile = searchQueue.Dequeue();
            searchedTilesSet.Add(searchTile);

            int searchTileDistance = distanceMap[searchTile];

            List<DungeonTile> connectedTiles = searchTile.ConnectedTiles;

            foreach (DungeonTile connectedTile in connectedTiles)
            {
                if (distanceMap.ContainsKey(connectedTile) == false)
                    distanceMap.Add(connectedTile, searchTileDistance + 1);

                if (searchedTilesSet.Contains(connectedTile) == false)
                    searchQueue.Enqueue(connectedTile);
            }
        }

        // From the end tile find the shortest path back to the start using the distance map
        DungeonTile nextTile = end;
        path.Add(end);

        while (nextTile != start)
        {
            List<DungeonTile> adjacentTiles = nextTile.ConnectedTiles;

            int bestNextTileDistance = int.MaxValue;

            foreach (DungeonTile adjacentTile in adjacentTiles)
            {
                if (distanceMap[adjacentTile] >= bestNextTileDistance)
                    continue;

                nextTile = adjacentTile;
                bestNextTileDistance = distanceMap[adjacentTile];
            }
            
            path.Add(nextTile);
        }

        path.Reverse();

        return path;
    }
}