using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int Seed = 10;
    public Transform TilePrefab;
    public Transform ObstaclePrefab;
    public Vector2 MapSize;
    [Range(0, 1)]
    public float OutlinePercent;

    const string HOLDER_NAME = "Generated Map";

    List<Coordinate> _allTileCoordinates;
    Queue<Coordinate> _shuffledCoordinates;

    void Start()
    {
        GeneratorMap();
    }

    public void GeneratorMap()
    {
        _allTileCoordinates = new List<Coordinate>();
        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                _allTileCoordinates.Add(new Coordinate(x, y));
            }
        }

        _shuffledCoordinates = new Queue<Coordinate>(Utility.Shuffle(_allTileCoordinates.ToArray(), Seed));

        // Destroy group
        Transform mapHolder = transform.Find(HOLDER_NAME); // FindChild was obsolete
        if (mapHolder)
        {
            DestroyImmediate(mapHolder.gameObject); // DestroyImmediate because we want to use it from the editor (instead of Destroy)
        }

        // (Re)Create group
        mapHolder = new GameObject(HOLDER_NAME).transform;
        mapHolder.parent = transform;

        // Create tiles
        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                var tilePosition = CoordinateToPosition(x, y);
                var newTile = Instantiate(TilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - OutlinePercent);

                newTile.parent = mapHolder; // Group


            }
        }

        // Create obstacles
        int obstacleCount = 10;
        for (int i = 0; i < obstacleCount; i++)
        {
            var randomCoordinator = GetRandomCoordinate();
            var obstaclePosition = CoordinateToPosition(randomCoordinator.x, randomCoordinator.y);
            var newObstacle = Instantiate(ObstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
            newObstacle.parent = mapHolder;//group
        }
    }

    private Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-MapSize.x / 2 + 0.5f + x, 0, -MapSize.y / 2 + 0.5f + y); // Center each tile
    }

    public Coordinate GetRandomCoordinate()
    {
        var randomCoordinate = _shuffledCoordinates.Dequeue();
        _shuffledCoordinates.Enqueue(randomCoordinate);

        return randomCoordinate;
    }

    public struct Coordinate
    {
        public int x, y;
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
