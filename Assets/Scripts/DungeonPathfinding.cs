using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPathfinding : MonoBehaviour
{

    private Dictionary<TileBag, TileBag[]> _adjacencyCache = new Dictionary<TileBag, TileBag[]>();

    public void SetAdjacencyCacheFromDungeon(Dungeon dungeon)
    {

    }


    public List<DungeonTile> GetShortestPath(DungeonTile start, DungeonTile end, Dungeon dungeon)
    {





    }
}
