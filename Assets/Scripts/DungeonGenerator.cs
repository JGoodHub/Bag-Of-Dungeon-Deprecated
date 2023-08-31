using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{


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

    [SerializeField] private int _seed;
    [SerializeField] private TileBag _tileBag;
    [Space]
    [SerializeField] private Transform _tilesContainer;
    [Space]
    [SerializeField] private GameObject _startTilePrefab;
    [SerializeField] private GameObject _endTilePrefab;
    [SerializeField] private GameObject _cornerTilePrefab;
    [SerializeField] private GameObject _straightTilePrefab;
    [SerializeField] private GameObject _crossTilePrefab;
    [SerializeField] private GameObject _capTilePrefab;

    private void Start()
    {
        _seed = _seed < 0 ? DateTime.Now.GetHashCode() : _seed;

        Random.InitState(_seed);

        _tileBag.Initialise();

        StartCoroutine(GenerateDungeonFromBag(_tileBag));
    }

    private void ClearCurrentDungeon()
    {
        foreach (Transform child in _tilesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator GenerateDungeonFromBag(TileBag bag)
    {
        ClearCurrentDungeon();

        List<Vector3Int> occupiedSpaces = new List<Vector3Int>();
        List<Vector3Int> availableSpaces = new List<Vector3Int> {Vector3Int.zero};

        List<DungeonTile> allPlacedTiles = new List<DungeonTile>();

        while (bag.IsEmpty() == false)
        {
            TileBag.TileType drawnTile = bag.DrawTile();
            GameObject tilePrefab = GetPrefabForTileType(drawnTile);
            DungeonTile dungeonTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();


            Vector3Int chosenSpace = availableSpaces[0];

            foreach (Vector3Int availableSpace in availableSpaces)
            {
                if (availableSpace.magnitude < chosenSpace.magnitude)
                    chosenSpace = availableSpace;
            }

            availableSpaces.Remove(chosenSpace);
            occupiedSpaces.Add(chosenSpace);

            dungeonTile.transform.position = chosenSpace;


            List<Vector3> existingConnections = allPlacedTiles.SelectMany(tile => tile.GetConnectors(true)).ToList();

            int bestRotation = 0;
            int maxConnections = -1;
            for (int r = 0; r < 4; r++)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                List<Vector3> tileConnectors = dungeonTile.GetConnectors(true);

                List<Vector3> matchingConnectors = existingConnections.Where(existCon => tileConnectors.Exists(tileCon => Vector3.Distance(tileCon, existCon) < 0.1f)).ToList();

                if (matchingConnectors.Count > maxConnections)
                {
                    bestRotation = r;
                    maxConnections = matchingConnectors.Count;
                }
            }


            dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * bestRotation, 0);
            allPlacedTiles.Add(dungeonTile);

            if (drawnTile == TileBag.TileType.Start)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, -90, 0);
            }


            List<Vector3Int> potentialAvailablePositions = dungeonTile.GetSurroundingTilePositions(true);
            potentialAvailablePositions.RemoveAll(potentialPosition => occupiedSpaces.Contains(potentialPosition));
            potentialAvailablePositions.RemoveAll(potentialPosition => availableSpaces.Contains(potentialPosition));

            availableSpaces.AddRange(potentialAvailablePositions);

            yield return new WaitForSeconds(0.1f);
        }

        foreach (Vector3Int availableSpace in availableSpaces)
        {
            GameObject tilePrefab = GetPrefabForTileType(TileBag.TileType.Cap);
            DungeonTile dungeonTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();

            occupiedSpaces.Add(availableSpace);

            dungeonTile.transform.position = availableSpace;


            List<Vector3> existingConnections = allPlacedTiles.SelectMany(tile => tile.GetConnectors(true)).ToList();

            int bestRotation = 0;
            int maxConnections = -1;
            for (int r = 0; r < 4; r++)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                List<Vector3> tileConnectors = dungeonTile.GetConnectors(true);

                List<Vector3> matchingConnectors = existingConnections.Where(existCon => tileConnectors.Exists(tileCon => Vector3.Distance(tileCon, existCon) < 0.1f)).ToList();

                if (matchingConnectors.Count > maxConnections)
                {
                    bestRotation = r;
                    maxConnections = matchingConnectors.Count;
                }
            }

            dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * bestRotation, 0);
            allPlacedTiles.Add(dungeonTile);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        availableSpaces.Clear();

    }

    private GameObject GetPrefabForTileType(TileBag.TileType tileType)
    {
        switch (tileType)
        {
            case TileBag.TileType.Start:
                return _startTilePrefab;
            case TileBag.TileType.End:
                return _endTilePrefab;
            case TileBag.TileType.Corner:
                return _cornerTilePrefab;
            case TileBag.TileType.Straight:
                return _straightTilePrefab;
            case TileBag.TileType.Cross:
                return _crossTilePrefab;
            case TileBag.TileType.Cap:
                return _capTilePrefab;
            default:
                throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
        }
    }

}
