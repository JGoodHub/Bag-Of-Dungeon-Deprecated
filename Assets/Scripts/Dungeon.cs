using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    private List<DungeonTile> _tiles;

    private DungeonPathfinding _pathfinding;

    public DungeonPathfinding Pathfinding => _pathfinding;

    public void AddTile(DungeonTile newTile)
    {
        _tiles ??= new List<DungeonTile>();
        _tiles.Add(newTile);
    }

    public void FinaliseSetup()
    {
        _pathfinding = new DungeonPathfinding(_tiles);
    }

    public DungeonTile GetStartTile()
    {
        return _tiles[0];
    }

    public DungeonTile GetRandomTile()
    {
        return _tiles[Random.Range(0, _tiles.Count)];
    }
}