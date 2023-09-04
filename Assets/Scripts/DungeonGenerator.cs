using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
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

    private Dungeon _dungeonInstance;

    private void Start()
    {
        _seed = _seed < 0 ? DateTime.Now.GetHashCode() : _seed;

        Random.InitState(_seed);

        _tileBag.Initialise();

        _dungeonInstance = GenerateDungeonInstanceFromBag(_tileBag);
    }

    private Dungeon GenerateDungeonInstanceFromBag(TileBag bag)
    {
        // Create the instance object
        GameObject dungeonInstanceObject = new GameObject("DungeonInstance");
        Dungeon dungeonInstance = dungeonInstanceObject.AddComponent<Dungeon>();

        List<Vector3Int> occupiedSpaces = new List<Vector3Int>();
        List<Vector3Int> availableSpaces = new List<Vector3Int> { Vector3Int.zero };

        List<DungeonTile> allPlacedTiles = new List<DungeonTile>();

        while (bag.IsEmpty() == false)
        {
            // Draw a tile and create it's object
            TileBag.TileType drawnTile = bag.DrawTile();
            GameObject tilePrefab = GetPrefabForTileType(drawnTile);
            DungeonTile dungeonTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();

            // Find the most central available space (keeps the dungeon compact/circular)
            Vector3Int chosenSpace = availableSpaces[0];
            foreach (Vector3Int availableSpace in availableSpaces)
                if (availableSpace.magnitude < chosenSpace.magnitude)
                    chosenSpace = availableSpace;

            // Move the tile to that space
            availableSpaces.Remove(chosenSpace);
            occupiedSpaces.Add(chosenSpace);

            dungeonTile.transform.position = chosenSpace;

            // Find the rotation that yields the most connected sides
            // The start tiles always faces up so we can skip thios for that tile
            if (drawnTile == TileBag.TileType.Start)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                List<Vector3> allOtherConnectors = new List<Vector3>();

                foreach (DungeonTile tile in allPlacedTiles)
                    allOtherConnectors.AddRange(tile.GetConnectors(true));


                int bestRotation = 0;
                int maxConnections = -1;
                for (int r = 0; r < 4; r++)
                {
                    dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                    List<Vector3> newTileConnectors = dungeonTile.GetConnectors(true);
                    int matchingConnectors = 0;

                    foreach (Vector3 otherConnector in allOtherConnectors)
                        if (newTileConnectors.Exists(newTileConnector => (newTileConnector - otherConnector).sqrMagnitude < 0.01f))
                            matchingConnectors++;

                    if (matchingConnectors <= maxConnections)
                        continue;

                    bestRotation = r;
                    maxConnections = matchingConnectors;

                    if (matchingConnectors == newTileConnectors.Count)
                        break;
                }

                dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * bestRotation, 0);
            }

            allPlacedTiles.Add(dungeonTile);

            // Add the newly available positions (that aren't already occupied) to the search area
            List<Vector3Int> potentialAvailablePositions = dungeonTile.GetSurroundingTilePositions(true);
            potentialAvailablePositions.RemoveAll(potentialPosition => occupiedSpaces.Contains(potentialPosition));
            potentialAvailablePositions.RemoveAll(potentialPosition => availableSpaces.Contains(potentialPosition));

            availableSpaces.AddRange(potentialAvailablePositions);
        }

        // Cap all the open corridors
        foreach (Vector3Int availableSpace in availableSpaces)
        {
            // Create the end cap object
            GameObject tilePrefab = GetPrefabForTileType(TileBag.TileType.Cap);
            DungeonTile dungeonTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();

            occupiedSpaces.Add(availableSpace);

            dungeonTile.transform.position = availableSpace;

            // Rotate the end cap to face it's matching tile
            List<Vector3> allOtherConnectors = new List<Vector3>();

            foreach (DungeonTile tile in allPlacedTiles)
                allOtherConnectors.AddRange(tile.GetConnectors(true));

            for (int r = 0; r < 4; r++)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                List<Vector3> newTileConnectors = dungeonTile.GetConnectors(true);
                int matchingConnectors = 0;

                foreach (Vector3 otherConnector in allOtherConnectors)
                    if (newTileConnectors.Exists(newTileConnector => (newTileConnector - otherConnector).sqrMagnitude < 0.01f))
                        matchingConnectors++;

                if (matchingConnectors == 1)
                    continue;
            }

            allPlacedTiles.Add(dungeonTile);
        }

        availableSpaces.Clear();

        // Hide all the tiles
        foreach (DungeonTile tile in allPlacedTiles)
        {
            dungeonInstance.AddTileToInstance(tile);

            if (tile.TileType != TileBag.TileType.Start)
                tile.Hide();

            List<Vector3Int> surroundingTilePositions = tile.GetSurroundingTilePositions(true);

            foreach (DungeonTile otherTile in allPlacedTiles)
            {
                if (tile == otherTile)
                    continue;

                foreach (Vector3Int surroundingTilePosition in surroundingTilePositions)
                {
                    if ((otherTile.transform.position - surroundingTilePosition).sqrMagnitude >= 0.01f)
                        continue;

                    tile.AdjacentTiles.Add(otherTile);
                    break;
                }
            }
        }

        dungeonInstance.FinaliseSetup();

        return dungeonInstance;
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