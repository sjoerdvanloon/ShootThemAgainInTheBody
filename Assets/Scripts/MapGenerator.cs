using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int Seed = 10;
    public Transform navMeshFloor;
    public Transform TilePrefab;
    public Transform ObstaclePrefab;
    public Transform NavMeshMaskPrefab;
    public Vector2 MapSize;
    public Vector2 MaxMapSize;
    [Range(0, 1)]
    public float OutlinePercent;
    [Range(0, 1)]
    public float ObstaclePercent;
    public float TileSize;

    const string HOLDER_NAME = "Generated Map";

    List<Coordinate> _allTileCoordinates;
    Queue<Coordinate> _shuffledCoordinates;
    Coordinate _mapCentre;

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

        // Get centre
        _mapCentre = new Coordinate((int)MapSize.x / 2, (int)MapSize.y / 2);

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
                newTile.localScale = Vector3.one * (1 - OutlinePercent) * TileSize;

                newTile.parent = mapHolder; // Group


            }
        }

        // Create obstacles
        bool[,] obstacleMap = new bool[(int)MapSize.x, (int)MapSize.y];
        int obstacleCount = (int)(MapSize.x * MapSize.y * ObstaclePercent); // Mapsize times obstacles percent
        var currentObstacleCount = 0;
        for (int i = 0; i < obstacleCount; i++)
        {
            var randomCoordinate = GetRandomCoordinate();

            // Prep
            obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
            currentObstacleCount++;

            // Check if it can happend
            if (randomCoordinate != _mapCentre && MapIsFullyAccesible(obstacleMap, currentObstacleCount))
            {
                // It could, so start rendering
                var obstaclePosition = CoordinateToPosition(randomCoordinate.x, randomCoordinate.y);

                var newObstacle = Instantiate(ObstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
                newObstacle.localScale = Vector3.one * (1 - OutlinePercent) * TileSize;
                newObstacle.parent = mapHolder; //group
            }
            else
            {
                // Could not happen, so reset
                currentObstacleCount--;
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
            }
        }

        var maskLeft = Instantiate(NavMeshMaskPrefab, Vector3.left * (MapSize.x + MaxMapSize.x) / 4 * TileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((MaxMapSize.x - MapSize.x) / 2, 1, MapSize.y) * TileSize;

        var maskRight = Instantiate(NavMeshMaskPrefab, Vector3.right * (MapSize.x + MaxMapSize.x) / 4 * TileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((MaxMapSize.x - MapSize.x) / 2, 1, MapSize.y) * TileSize;


        var maskTop = Instantiate(NavMeshMaskPrefab, Vector3.forward * (MapSize.y + MaxMapSize.y) / 4 * TileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - MapSize.y)/2) * TileSize;

        var maskBottom = Instantiate(NavMeshMaskPrefab, Vector3.back * (MapSize.y + MaxMapSize.y) / 4 * TileSize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - MapSize.y) / 2) * TileSize;

        navMeshFloor.localScale = new Vector3(MaxMapSize.x, MaxMapSize.y) * 2;
    }

    private Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-MapSize.x / 2 + 0.5f + x, 0, -MapSize.y / 2 + 0.5f + y) * TileSize; // Center each tile
    }

    bool MapIsFullyAccesible(bool[,] obstacleMap, int currentObstacleCount)
    {
        // Flood fill algo
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(_mapCentre);
        mapFlags[_mapCentre.x, _mapCentre.y] = true; // We know it is empty

        var accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue(); // First item from the queue

            // Loop through neightbour 8 tiles
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var neighBourX = tile.x + x;
                    var neighBourY = tile.y + y;

                    var dialognally = (x == 0 || y == 0);
                    if (dialognally)
                    {
                        var insideObstacleMap = (neighBourX >= 0 && neighBourX < obstacleMap.GetLength(0) && neighBourY >= 0 && neighBourY < obstacleMap.GetLength(1));
                        if (insideObstacleMap)
                        {
                            var mappedFlag = mapFlags[neighBourX, neighBourY];
                            var isObstacleTile = obstacleMap[neighBourX, neighBourY];

                            if (!mappedFlag && !isObstacleTile)
                            {
                                mapFlags[neighBourX, neighBourY] = true; // Have checked
                                queue.Enqueue(new Coordinate(neighBourX, neighBourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(MapSize.x * MapSize.y - currentObstacleCount);

        return targetAccessibleTileCount == accessibleTileCount;
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

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }


        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return c1.x != c2.x || c1.y == c2.y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Coordinate coord)
            {
                return this == coord;
            }

            return false;
        }

        public override int GetHashCode() => new { x, y }.GetHashCode();

    }
}
