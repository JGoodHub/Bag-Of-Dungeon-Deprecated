using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{

    private List<DungeonTile> _tiles;

    private DungeonPathfinding _pathfinding;

    public DungeonPathfinding Pathfinding => _pathfinding;

    public void AddTileToInstance(DungeonTile newTile)
    {
        _tiles.Add(newTile);
    }


    public void FinaliseSetup()
    {





    }


}