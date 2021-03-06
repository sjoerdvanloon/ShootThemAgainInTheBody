using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] Maps;
    public int MapIndex;
    public Transform navMeshFloor;
    public Transform MapFloor;
    public Transform TilePrefab;
    public Transform ObstaclePrefab;
    public Transform NavMeshMaskPrefab;
    public Vector2 MaxMapSize;
    [Range(0, 1)]
    public float OutlinePercent;
    public float TileSize;

    const string HOLDER_NAME = "Generated Map";

    List<Coordinate> _allTileCoordinates;
    Queue<Coordinate> _shuffledTileCoordinates;
    Queue<Coordinate> _shuffledOpenTileCoordinates;
    Transform[,] _tileMap;

    Map CurrentMap { get => Maps[MapIndex]; }
   
    // Not sure why I can;t do this 
    //System.Random Random
    //{
    //    get
    //    {
    //        if (_lastSeed != CurrentMap.Seed || _lastRandom == null)
    //        {
    //            print($"new random based on {CurrentMap.Seed}");
    //            _lastRandom = new System.Random(CurrentMap.Seed);
    //            _lastSeed = CurrentMap.Seed;
    //        }

    //        return _lastRandom;
    //    }
    //}

    //int _lastSeed = -1;
    //System.Random _lastRandom = null;


    void Start()
    {
        var spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        MapIndex = waveNumber -1;
        GeneratorMap();
    }

    public void GeneratorMap()
    {
        // Set tilemap
        _tileMap = new Transform[CurrentMap.MapSize.x, CurrentMap.MapSize.y];


        // Set box collider on ground
        //GetComponent<BoxCollider>().size = new Vector3(CurrentMap.MapSize.x * TileSize, .05f, CurrentMap.MapSize.y * TileSize); // Removed because of https://youtu.be/8fZIZMlC69s?t=506

        var mapHolder = RegenerateMapHolder();

        // Generating coordinates
        _allTileCoordinates = new List<Coordinate>();
        for (int x = 0; x < CurrentMap.MapSize.x; x++)
        {
            for (int y = 0; y < CurrentMap.MapSize.y; y++)
            {
                _allTileCoordinates.Add(new Coordinate(x, y));
            }
        }
        _shuffledTileCoordinates = new Queue<Coordinate>(Utility.Shuffle(_allTileCoordinates.ToArray(), CurrentMap.Seed));


        // Create tiles
        CreateTiles(mapHolder);

        // Create obstacles
        CreateObstacles(mapHolder);



        // Creating the masks
        CreateFloorMasks(mapHolder);

        navMeshFloor.localScale = new Vector3(MaxMapSize.x, MaxMapSize.y) * 2;
        MapFloor.localScale = new Vector3(CurrentMap.MapSize.x * TileSize, CurrentMap.MapSize.y * TileSize);
    }

    private Transform RegenerateMapHolder()
    {
        // Destroy group
        Transform mapHolder = transform.Find(HOLDER_NAME); // FindChild was obsolete
        if (mapHolder)
        {
            DestroyImmediate(mapHolder.gameObject); // DestroyImmediate because we want to use it from the editor (instead of Destroy)
        }

        // (Re)Create group
        mapHolder = new GameObject(HOLDER_NAME).transform;
        mapHolder.parent = transform;
        return mapHolder;
    }

    private void CreateTiles(Transform mapHolder)
    {
        for (int x = 0; x < CurrentMap.MapSize.x; x++)
        {
            for (int y = 0; y < CurrentMap.MapSize.y; y++)
            {
                var tilePosition = CoordinateToPosition(x, y);
                var newTile = Instantiate(TilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - OutlinePercent) * TileSize;

                newTile.parent = mapHolder; // Group

                // DriitzzCabel
                Renderer newTileRenderer = newTile.GetComponent<Renderer>();
                Material newTileMaterial = new Material(newTileRenderer.sharedMaterial);
                newTileMaterial.color = CurrentMap.FloorColor;
                newTileRenderer.sharedMaterial = newTileMaterial;

                _tileMap[x, y] = newTile;
            }
        }
    }

    private void CreateObstacles(Transform mapHolder)
    {
        var Random = new System.Random(CurrentMap.Seed);
        bool[,] obstacleMap = new bool[CurrentMap.MapSize.x, CurrentMap.MapSize.y];
        int obstacleCount = (int)(CurrentMap.MapSize.x * CurrentMap.MapSize.y * CurrentMap.ObstaclePercent); // Mapsize times obstacles percent
        var currentObstacleCount = 0;
        var  allOpenCoordinates = new List<Coordinate>(_allTileCoordinates);

        var randList = new List<float>();
        for (int i = 0; i < obstacleCount; i++)
        {
            var randomCoordinate = GetRandomCoordinate();

            // Prep
            obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
            currentObstacleCount++;

            // Check if it can happened
            if (randomCoordinate != CurrentMap.MapCentre && MapIsFullyAccesible(obstacleMap, currentObstacleCount))
            {
                var randomHeightPercentage = (float)Random.NextDouble();
                randList.Add(randomHeightPercentage);
                var obstacleHeight = Mathf.Lerp(CurrentMap.minObstacleHeight, CurrentMap.maxObstacleHeight, randomHeightPercentage);
                // It could, so start rendering
                var obstaclePosition = CoordinateToPosition(randomCoordinate.x, randomCoordinate.y);

                var newObstacle = Instantiate(ObstaclePrefab, obstaclePosition + Vector3.up * (obstacleHeight / 2f), Quaternion.identity);
                newObstacle.localScale = new Vector3(
                    (1 - OutlinePercent) * TileSize, 
                    obstacleHeight, 
                    (1-OutlinePercent) * TileSize);
                newObstacle.parent = mapHolder; //group

                Renderer renderer = newObstacle.GetComponent<Renderer>();
                Material material = new Material(renderer.sharedMaterial);
                float colorPercentage = randomCoordinate.y / (float)CurrentMap.MapSize.y; // has to be cast, else no float will be returned
                material.color = Color.Lerp(CurrentMap.ForegroundColor,CurrentMap.BackgroundColor, colorPercentage);
                renderer.sharedMaterial = material;

                // Remove from open coords
                allOpenCoordinates.Remove(randomCoordinate);
            }
            else
            {
                // Could not happen, so reset
                currentObstacleCount--;
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
            }
        }

        _shuffledOpenTileCoordinates = new Queue<Coordinate>(Utility.Shuffle(allOpenCoordinates.ToArray(), CurrentMap.Seed));


        // print(string.Join(", ", randList));
    }

    private void CreateFloorMasks(Transform mapHolder)
    {
        var maskLeft = Instantiate(NavMeshMaskPrefab, Vector3.left * (CurrentMap.MapSize.x + MaxMapSize.x) / 4f * TileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((MaxMapSize.x - CurrentMap.MapSize.x) / 2f, 1, CurrentMap.MapSize.y) * TileSize;

        var maskRight = Instantiate(NavMeshMaskPrefab, Vector3.right * (CurrentMap.MapSize.x + MaxMapSize.x) / 4f * TileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((MaxMapSize.x - CurrentMap.MapSize.x) / 2f, 1, CurrentMap.MapSize.y) * TileSize;


        var maskTop = Instantiate(NavMeshMaskPrefab, Vector3.forward * (CurrentMap.MapSize.y + MaxMapSize.y) / 4f * TileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - CurrentMap.MapSize.y) / 2f) * TileSize;

        var maskBottom = Instantiate(NavMeshMaskPrefab, Vector3.back * (CurrentMap.MapSize.y + MaxMapSize.y) / 4f * TileSize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - CurrentMap.MapSize.y) / 2f) * TileSize;
    }

    private Vector3 CoordinateToPosition(int x, int y)
    {
        return new Vector3(-CurrentMap.MapSize.x / 2f + 0.5f + x, 0, -CurrentMap.MapSize.y / 2f + 0.5f + y) * TileSize; // Center each tile
    }

    bool MapIsFullyAccesible(bool[,] obstacleMap, int currentObstacleCount)
    {
        // Flood fill algo
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(CurrentMap.MapCentre);
        mapFlags[CurrentMap.MapCentre.x, CurrentMap.MapCentre.y] = true; // We know it is empty

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
        int targetAccessibleTileCount = (int)(CurrentMap.MapSize.x * CurrentMap.MapSize.y - currentObstacleCount);

        return targetAccessibleTileCount == accessibleTileCount;
    }

    public Coordinate GetRandomCoordinate()
    {
        var randomCoordinate = _shuffledTileCoordinates.Dequeue();
        _shuffledTileCoordinates.Enqueue(randomCoordinate);

        return randomCoordinate;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt( position.x / TileSize + (CurrentMap.MapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt( position.z / TileSize + (CurrentMap.MapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, _tileMap.GetLength(0)-1);   
        y = Mathf.Clamp(y, 0, _tileMap.GetLength(1)-1);   
        return _tileMap[x, y];
    }

    public Transform GetRandomOpenTile()
    {
        var randomCoordinate = _shuffledOpenTileCoordinates.Dequeue();
        _shuffledOpenTileCoordinates.Enqueue(randomCoordinate);

        return _tileMap[randomCoordinate.x, randomCoordinate.y];
    }

    [System.Serializable]
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

    [System.Serializable]
    public class Map
    {
        public Coordinate MapSize;
        [Range(0, 1)]
        public float ObstaclePercent;
        public int Seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color ForegroundColor;
        public Color BackgroundColor;
        public Color FloorColor;  // DriitzzCabal https://www.youtube.com/watch?v=9bYy1uqHY0Q
        public Coordinate MapCentre { get { return new Coordinate(MapSize.x / 2, MapSize.y / 2); } }
    }
}
