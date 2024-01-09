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

    private Dungeon _dungeon;

    private void Start()
    {
        _seed = _seed < 0 ? DateTime.Now.GetHashCode() : _seed;

        Random.InitState(_seed);

        _tileBag.Initialise();

        _dungeon = GenerateDungeonInstanceFromBag(_tileBag);

        List<DungeonTile> shortestPath = _dungeon.Pathfinding.GetShortestPath(_dungeon.GetStartTile(), _dungeon.GetRandomTile());

        for (int i = 0; i < shortestPath.Count - 1; i++)
        {
            Debug.DrawLine(shortestPath[i].transform.position + Vector3.up, shortestPath[i + 1].transform.position + Vector3.up, Color.red, 50f);
        }
    }

    private Dungeon GenerateDungeonInstanceFromBag(TileBag bag)
    {
        // Create the instance object
        GameObject dungeonInstanceObject = new GameObject("DungeonInstance");
        Dungeon dungeon = dungeonInstanceObject.AddComponent<Dungeon>();

        // Create the start tile

        GameObject startTilePrefab = GetPrefabForTileType(TileBag.TileType.Start);
        DungeonTile startDungeonTile = Instantiate(startTilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();

        List<Vector3Int> occupiedSpaces = new List<Vector3Int> {Vector3Int.zero};
        List<DungeonTile> edgeTiles = new List<DungeonTile> {startDungeonTile};

        List<Vector3Int> availableSpaces = new List<Vector3Int>();
        availableSpaces.AddRange(startDungeonTile.GetAdjacentTilePositions(true));

        List<DungeonTile> tiles = new List<DungeonTile>();

        while (bag.IsEmpty() == false)
        {
            // Draw a tile and create it's object
            TileBag.TileType drawnTileType = bag.DrawTile();
            GameObject tilePrefab = GetPrefabForTileType(drawnTileType);
            DungeonTile dungeonTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, _tilesContainer).GetComponent<DungeonTile>();

            // Find the most central available space (keeps the dungeon compact/circular)
            DungeonTile mostCentralTile = edgeTiles[0];
            foreach (DungeonTile edgeTile in edgeTiles)
                if (edgeTile.transform.position.magnitude < mostCentralTile.transform.position.magnitude)
                    mostCentralTile = edgeTile;

            List<Vector3Int> centralTileAdjacentSpaces = mostCentralTile.GetAdjacentTilePositions(true);
            Vector3Int chosenSpace = centralTileAdjacentSpaces[Random.Range(0, centralTileAdjacentSpaces.Count)];
            
            dungeonTile.gameObject.name = $"DungeonTile_({drawnTileType} / {chosenSpace.x}, {chosenSpace.z})";

            // Move the tile to that space
            availableSpaces.Remove(chosenSpace);
            occupiedSpaces.Add(chosenSpace);

            dungeonTile.transform.position = chosenSpace;

            // Find the rotation that yields the most connected sides
            // The start tiles always faces up so we can skip this for that tile
            if (drawnTileType == TileBag.TileType.Start)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                List<Vector3> allOtherConnectors = new List<Vector3>();

                foreach (DungeonTile tile in tiles)
                    allOtherConnectors.AddRange(tile.GetConnectorPositions(true));

                int bestRotation = 0;
                int maxConnections = -1;
                for (int r = 0; r < 4; r++)
                {
                    dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                    List<Vector3> newTileConnectors = dungeonTile.GetConnectorPositions(true);
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

            tiles.Add(dungeonTile);

            // Add the newly available positions (that aren't already occupied) to the search area
            List<Vector3Int> potentialAvailablePositions = dungeonTile.GetAdjacentTilePositions(true);
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

            dungeonTile.gameObject.name = $"DungeonTile_({TileBag.TileType.Cap} / {availableSpace.x}, {availableSpace.z})";

            // Rotate the end cap to face it's matching tile
            List<Vector3> allOtherConnectors = new List<Vector3>();

            foreach (DungeonTile tile in tiles)
                allOtherConnectors.AddRange(tile.GetConnectorPositions(true));

            for (int r = 0; r < 4; r++)
            {
                dungeonTile.transform.rotation = Quaternion.Euler(0, 90 * r, 0);

                List<Vector3> newTileConnectors = dungeonTile.GetConnectorPositions(true);
                int matchingConnectors = 0;

                foreach (Vector3 otherConnector in allOtherConnectors)
                    if (newTileConnectors.Exists(newTileConnector => (newTileConnector - otherConnector).sqrMagnitude < 0.01f))
                        matchingConnectors++;

                if (matchingConnectors == 1)
                    break;
            }

            tiles.Add(dungeonTile);
        }

        availableSpaces.Clear();

        // Hide all the tiles
        foreach (DungeonTile tile in tiles)
        {
            dungeon.AddTile(tile);

            if (tile.TileType != TileBag.TileType.Start)
                tile.Hide();

            // Get a list of the all the tiles connected to this one
            List<Vector3Int> connectedTilePositions = tile.GetAdjacentTilePositions(true);

            foreach (DungeonTile otherTile in tiles)
            {
                if (tile == otherTile)
                    continue;

                foreach (Vector3Int connectedTilePosition in connectedTilePositions)
                {
                    if ((otherTile.transform.position - connectedTilePosition).sqrMagnitude >= 0.01f)
                        continue;

                    tile.ConnectedTiles.Add(otherTile);
                    break;
                }
            }
        }

        dungeon.FinaliseSetup();

        return dungeon;
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