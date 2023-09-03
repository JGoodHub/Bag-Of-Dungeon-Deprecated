using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class TileBag
{
    public enum TileType
    {
        Start,
        End,
        Corner,
        Straight,
        Cross,
        Cap
    }

    public int cornerTileCount;
    public int straightTileCount;
    public int crossTileCount;

    private List<TileType> _bag;

    public void Initialise()
    {
        _bag = new List<TileType>();

        for (int i = 0; i < cornerTileCount; i++)
            _bag.Add(TileType.Corner);

        for (int i = 0; i < straightTileCount; i++)
            _bag.Add(TileType.Straight);

        for (int i = 0; i < crossTileCount; i++)
            _bag.Add(TileType.Cross);


        for (int i = 0; i < _bag.Count * 10; i++)
        {
            int indexA = Random.Range(0, _bag.Count);
            int indexB = Random.Range(0, _bag.Count);

            TileType temp = _bag[indexA];
            _bag[indexA] = _bag[indexB];
            _bag[indexB] = temp;
        }


        _bag.Insert(0, TileType.Start);
        _bag.Insert(Random.Range(_bag.Count / 2, _bag.Count), TileType.End);
    }

    public TileType DrawTile()
    {
        if (_bag.Count == 0)
            throw new IndexOutOfRangeException();

        TileType drawnTile = _bag[0];
        _bag.RemoveAt(0);
        return drawnTile;
    }

    public TileType PeakTile()
    {
        if (_bag.Count == 0)
            throw new IndexOutOfRangeException();

        return _bag[0];
    }

    public bool IsEmpty()
    {
        return _bag.Count == 0;
    }
}